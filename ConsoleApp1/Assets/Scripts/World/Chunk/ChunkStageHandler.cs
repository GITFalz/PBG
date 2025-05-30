using System.Diagnostics;

public static class ChunkStageHandler
{
    public static void ExecuteStage(ChunkEntry entry)
    {
        switch (entry.Stage)
        {
            case ChunkStage.ToBeGenerated:
                entry.SetStage(ChunkStage.Generating);
                entry.SetWantedStage = false; 
                ChunkGenerationProcess generationProcess = new ChunkGenerationProcess(entry, false);
                ThreadPool.QueueAction(generationProcess, TaskPriority.Low);
                break;

            case ChunkStage.Generated:
                entry.SetStage(ChunkStage.ToBePopulated);
                break;

            case ChunkStage.ToBePopulated:
                if (!entry.Chunk.AllChunksStageBetween(ChunkStage.Generated, ChunkStage.Created))
                {
                    entry.SetTimer(0.5f);
                    break;
                }

                entry.SetStage(ChunkStage.Populating);
                entry.SetWantedStage = false;
                ChunkPopulationProcess populationProcess = new ChunkPopulationProcess(entry);
                ThreadPool.QueueAction(populationProcess, TaskPriority.Normal);
                break;

            case ChunkStage.Populated:
                entry.SetStage(ChunkStage.ToBeRendered);
                Info.TotalGenTime += entry.generationTime.ElapsedMilliseconds;
                Info.TotalGenCount++;
                Info.AvgChunkGenTime = Info.TotalGenTime / Info.TotalGenCount;
                break;

            case ChunkStage.ToBeRendered:
                entry.SetStage(ChunkStage.Rendering);
                entry.SetWantedStage = false;
                ChunkRenderingProcess renderingProcess = new ChunkRenderingProcess(entry);
                ThreadPool.QueueAction(renderingProcess, TaskPriority.High);
                break;

            case ChunkStage.Rendered:
                entry.SetStage(ChunkStage.ToBeCreated);
                break;

            case ChunkStage.ToBeCreated:
                entry.SetStage(ChunkStage.Creating);
                if (entry.Chunk.CreateChunkSolid())
                {
                    entry.SetStage(ChunkStage.Created);
                }
                else
                {
                    entry.SetStage(ChunkStage.ToBeFreed);
                }     
                break;

            case ChunkStage.Created:
                break;

            case ChunkStage.ToBeReloaded:
                entry.SetStage(ChunkStage.Reloading);
                break;

            case ChunkStage.Reloaded:
                break;

            case ChunkStage.ToBeFreed:
                entry.Chunk.Status = ChunkStatus.Hidden;
                ChunkManager.FreeChunk(entry);
                entry.SetStage(ChunkStage.Free);
                break;

            case ChunkStage.ToBeDeleted:
                entry.SetStage(ChunkStage.Deleting);
                break;

            case ChunkStage.Deleted:
                entry.SetStage(ChunkStage.Empty);
                break;

            default:
                break;
        }
    }
}

public enum ChunkStage
{
    Empty = -1,
    Free = 0,
    ToBeGenerated = 1,
    Generating = 2,
    Generated = 3,
    ToBePopulated = 4,
    Populating = 5,
    Populated = 6,
    ToBeRendered = 7,
    Rendering = 8,
    Rendered = 9,
    ToBeCreated = 10,
    Creating = 11,
    Created = 12,
    ToBeReloaded = 13,
    Reloading = 14,
    Reloaded = 15,
    ToBeFreed = 16,
    Freeing = 17,
    ToBeDeleted = 18,
    Deleting = 19,
    Deleted = 20,
}