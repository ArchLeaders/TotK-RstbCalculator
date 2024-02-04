using TotkRstbGenerator.Core.Services.CalculatorServices;

namespace TotkRstbGenerator.Core.Helpers;

public class ResourceSizeHelper
{
    public static uint EstimateSize(uint size, string canonical, string extension, Span<byte> data)
    {
        return canonical switch {
            "Event/EventFlow/Dm_ED_0004.bfevfl" => size + 0x1E0,
            "Effect/static.Nin_NX_NVN.esetb.byml" => size + 0x1000,
            "Effect/static.Product.110.Nin_NX_NVN.esetb.byml" => size + 0x1000,
            "Lib/agl/agl_resource.Nin_NX_NVN.release.sarc" => size + 0x1000,
            "Lib/gsys/gsys_resource.Nin_NX_NVN.release.sarc" => size + 0x1000,
            "Lib/Terrain/tera_resource.Nin_NX_NVN.release.sarc" => size + 0x1000,
            "Shader/ApplicationPackage.Nin_NX_NVN.release.sarc" => size + 0x1000,
            _ => extension switch {
                ".ainb" => size + AinbResourceCalculator.CalculateSizeOffset(data),
                ".asb" => size + AsbResourceCalculator.CalculateSizeOffset(data),
                ".bgyml" => (size + 1000) * 8,
                ".baatarc" => size + 0x100,
                ".baev" => size + 0x120,
                ".bagst" => size + 0x100,
                ".bars" => size + 0x240,
                ".bcul" => size + 0x100,
                ".beco" => size + 0x100,
                ".belnk" => size + 0x100,
                ".bfarc" => size + 0x100,
                ".bfevfl" => size + 0x120,
                ".bfsha" => size + 0x100,
                ".bhtmp" => size + 0x100,
                ".blal" => size + 0x100,
                ".blarc" => size + 0x100,
                ".blwp" => size + 0x100,
                ".bnsh" => size + 0x100,
                ".bntx" => size + 0x100,
                ".bphcl" => size + 0x100,
                ".bphhb" => size + 0x100,
                ".bphnm" => size + 0x120,
                ".bphsh" => size + 0x170,
                ".bslnk" => size + 0x100,
                ".bstar" => size + BstarResourceCalculator.CalculateSizeOffset(data),
                ".byml" => size + 0x100,
                ".cai" => size + 0x100,
                ".casset.byml" => size + 0x1C0,
                ".chunk" => size + 0x100,
                ".crbin" => size + 0x100,
                ".cutinfo" => size + 0x100,
                ".dpi" => size + 0x100,
                ".genvb" => size + 0x180,
                ".jpg" => size + 0x100,
                ".mc" => (size + ModelCodecResourceCalculator.CalculateSizeOffset(data) + 1500) * 4,
                ".pack" => size + 0x180,
                ".png" => size + 0x100,
                ".quad" => size + 0x100,
                ".sarc" => size + 0x180,
                ".tscb" => size + 0x100,
                ".txtg" => size + 0x100,
                ".txt" => size + 0x100,
                ".vsts" => size + 0x100,
                ".wbr" => size + 0x100,
                ".zs" => size + 0x100,
                _ => (size + 1500) * 4
            }
        };
    }
}
