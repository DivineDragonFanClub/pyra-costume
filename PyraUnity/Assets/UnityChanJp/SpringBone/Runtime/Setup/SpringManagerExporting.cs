using UTJ.Jobs;

namespace UnityChanJp.SpringBone.Runtime.Setup
{
    public class SpringManagerExporting(SpringJobManager manager)
    {
        var managerSerializer = SpringManagerImporting.SpringManagerSerializer(manager);
        builder.Append(boneSerializer);
    }
}