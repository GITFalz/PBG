public class Rig
{
    public RootBone RootBone = new RootBone("RootBone");
    public List<Bone> Bones = [];

    public Rig()
    {
        Create();
    }

    public void Create()
    {
        Bones = [];
        RootBone.GetBones(Bones);
    }

    public Bone Copy()
    {
        return RootBone.Copy();
    }
}