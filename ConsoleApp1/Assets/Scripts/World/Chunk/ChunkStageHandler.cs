public static class ChunkStageHandler
{
    public static void ExecuteStage(ChunkEntry entry)
    {
        
    }

    public static void SetStage(ChunkEntry entry, ChunkStage stage)
    {
        entry.SetStage(stage);
        entry.SetUpdateAction(_stageActions[stage]);
    }

    public static void Handle_Empty(ChunkEntry entry)
    {
        
    }

    public static void Handle_Free(ChunkEntry entry)
    {
        
    }

    public static void Handle_Loading(ChunkEntry entry)
    {
        
    }

    public static void Handle_Generated(ChunkEntry entry)
    {
        
    }

    public static void Handle_ReadyToBePopulated(ChunkEntry entry)
    {
        
    }

    public static void Handle_Populated(ChunkEntry entry)
    {
        
    }

    public static void Handle_ReadyToBeRendered(ChunkEntry entry)
    {
        
    }

    public static void Handle_Rendered(ChunkEntry entry)
    {
        
    }

    public static void Handle_ReadyToBeCreated(ChunkEntry entry)
    {
        
    }

    public static void Handle_Created(ChunkEntry entry)
    {
        
    }

    public static void Handle_ReadyToBeReloaded(ChunkEntry entry)
    {
        
    }

    public static void Handle_ToBeFreed(ChunkEntry entry)
    {
        
    }

    public static void Handle_ToBeDeleted(ChunkEntry entry)
    {
        
    }

    private static readonly Dictionary<ChunkStage, Action<ChunkEntry>> _stageActions = new()
    {
        { ChunkStage.Empty, Handle_Empty },
        { ChunkStage.Free, Handle_Free },
        { ChunkStage.Loading, Handle_Loading },
        { ChunkStage.Generated, Handle_Generated },
        { ChunkStage.ReadyToBePopulated, Handle_ReadyToBePopulated },
        { ChunkStage.Populated, Handle_Populated },
        { ChunkStage.ReadyToBeRendered, Handle_ReadyToBeRendered },
        { ChunkStage.Rendered, Handle_Rendered },
        { ChunkStage.ReadyToBeCreated, Handle_ReadyToBeCreated },
        { ChunkStage.Created, Handle_Created },
        { ChunkStage.ReadyToBeReloaded, Handle_ReadyToBeReloaded },
        { ChunkStage.ToBeFreed, Handle_ToBeFreed },
        { ChunkStage.ToBeDeleted, Handle_ToBeDeleted }
    };
}

public enum ChunkStage
{
    Empty = -1,
    Free = 0,
    Loading = 1,
    Generated = 2,
    ReadyToBePopulated = 3,
    Populated = 4,
    ReadyToBeRendered = 5,
    Rendered = 6,
    ReadyToBeCreated = 7,
    Created = 8,
    ReadyToBeReloaded = 9,
    ToBeFreed = 8,
    ToBeDeleted = 9,
}