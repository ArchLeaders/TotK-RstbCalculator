namespace TotkRstbGenerator.Core.Models;

// Source: https://github.com/MasterBubbles/restbl

public static class RstbInfo
{
    public static readonly Dictionary<string, int> ExceptionSizeOffsetMap = new() {
        { "Event/EventFlow/Dm_ED_0004.bfevfl", 480 },
        { "Effect/static.Nin_NX_NVN.esetb.byml", 4096 },
        { "Lib/agl/agl_resource.Nin_NX_NVN.release.sarc", 4096 },
        { "Lib/gsys/gsys_resource.Nin_NX_NVN.release.sarc", 4096 },
        { "Lib/Terrain/tera_resource.Nin_NX_NVN.release.sarc", 4096 },
        { "Lib/Shader/ApplicationPackage.Nin_NX_NVN.release.sarc", 4096 },
    };

    public static readonly Dictionary<string, int> SizeOffsetMap = new() {
        { ".baatarc", 256 },
        { ".baev", 288 },
        { ".bagst", 256 },
        { ".bars", 576 },
        { ".bcul", 256 },
        { ".beco", 256 },
        { ".belnk", 256 },
        { ".bfarc", 256 },
        { ".bfevfl", 288 },
        { ".bfsha", 256 },
        { ".bhtmp", 256 },
        { ".blal", 256 },
        { ".blarc", 256 },
        { ".blwp", 256 },
        { ".bnsh", 256 },
        { ".bntx", 256 },
        { ".bphcl", 256 },
        { ".bphhb", 256 },
        { ".bphnm", 288 },
        { ".bphsh", 368 },
        { ".bslnk", 256 },
        { ".byml", 256 },
        { ".cai", 256 },
        { ".casset.byml", 448 },
        { ".chunk", 256 },
        { ".crbin", 256 },
        { ".cutinfo", 256 },
        { ".dpi", 256 },
        { ".genvb", 384 },
        { ".jpg", 256 },
        { ".pack", 384 },
        { ".png", 256 },
        { ".quad", 256 },
        { ".sarc", 384 },
        { ".tscb", 256 },
        { ".txtg", 256 },
        { ".txt", 256 },
        { ".vsts", 256 },
        { ".wbr", 256 },
        { ".zs", 256 } // .ta.zs
    };
}
