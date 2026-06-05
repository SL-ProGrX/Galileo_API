
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos
{
    public static class CcProcesoMensualCargaDeduccionesConfig
    {
        private const int TipoAporte = 1;
        private const int TipoCredito = 3;

        public static IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> ObtenerReglas(
            CcProcesoMensualCargaDeduccionesTipo tipoCarga)
        {
            return tipoCarga switch
            {
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Excel_Tek_Experts => sbCargaDeduc_Excel_Tek_Experts,
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Excel_DxC_Costa_Rica => sbCargaDeduc_Excel_DxC_Costa_Rica,
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Excel => sbCargaDeduc_Excel,
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Excel_DxC_CentroAmerica =>  sbCargaDeduc_Excel_DxC_CentroAmerica,
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_ExcelNew => [],
                CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Csv_Integra => [],
                _ => []
            };
        }

        private static readonly IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> sbCargaDeduc_Excel_Tek_Experts =
        [
        new() { CodDeduccion = "02-A06", Tipo = TipoAporte,  ColumnasOrigen = ["02-A06"] },
        new() { CodDeduccion = "02-D30", Tipo = TipoAporte,  ColumnasOrigen = ["02-D30"] },
        new() { CodDeduccion = "02-D31", Tipo = TipoCredito, ColumnasOrigen = ["02-D31"] },
        new() { CodDeduccion = "02-D32", Tipo = TipoCredito, ColumnasOrigen = ["02-D32"] },
        new() { CodDeduccion = "02-D33", Tipo = TipoCredito, ColumnasOrigen = ["02-D33"] },
        new() { CodDeduccion = "02-D34", Tipo = TipoCredito, ColumnasOrigen = ["02-D34"] },
        new() { CodDeduccion = "02-D35", Tipo = TipoCredito, ColumnasOrigen = ["02-D35"] },
        new() { CodDeduccion = "02-D36", Tipo = TipoCredito, ColumnasOrigen = ["02-D36"] },
        new() { CodDeduccion = "02-D37", Tipo = TipoCredito, ColumnasOrigen = ["02-D37"] },
        new() { CodDeduccion = "02-D38", Tipo = TipoCredito, ColumnasOrigen = ["02-D38"] }
        ];

        private static readonly IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> sbCargaDeduc_Excel_DxC_Costa_Rica =
        [
        new() { CodDeduccion = "DE16", Tipo = TipoAporte,  ColumnasOrigen = ["2"] },
        new() { CodDeduccion = "DE15", Tipo = TipoAporte,  ColumnasOrigen = ["12"] },
        new() { CodDeduccion = "DE31", Tipo = TipoCredito, ColumnasOrigen = ["11"] },
        new() { CodDeduccion = "DE17", Tipo = TipoCredito, ColumnasOrigen = ["4", "5", "6", "7", "8"] },
        new() { CodDeduccion = "DE14", Tipo = TipoCredito, ColumnasOrigen = ["3", "10"] },
        new() { CodDeduccion = "DE24", Tipo = TipoCredito, ColumnasOrigen = ["9"] }
        ];

        private static readonly IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> sbCargaDeduc_Excel_DxC_CentroAmerica =
        [
            new() { CodDeduccion = "DE16", Tipo = TipoAporte, ColumnasOrigen = ["3"] },
            new() { CodDeduccion = "DE15", Tipo = TipoAporte, ColumnasOrigen = ["12"] },

            new() { CodDeduccion = "DE31", Tipo = TipoCredito, ColumnasOrigen = ["11"] },
            new() { CodDeduccion = "DE17", Tipo = TipoCredito, ColumnasOrigen = ["4", "5", "7", "9"] },
            new() { CodDeduccion = "DE14", Tipo = TipoCredito, ColumnasOrigen = ["2", "8"] },
            new() { CodDeduccion = "DE24", Tipo = TipoCredito, ColumnasOrigen = ["6"] }
        ];


        private static readonly IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> sbCargaDeduc_Excel =
        [
            new()
            {
                CodDeduccion = "O",
                Tipo = TipoAporte,
                ColumnasOrigen = ["APORTES"],
                RequiereAportesHabilitados = true,
                InsertaSoloSiMontoMayorQueCero = false
            },
            new()
            {
                CodDeduccion = "P",
                Tipo = TipoAporte,
                ColumnasOrigen = ["PATRONAL"],
                RequiereAportesHabilitados = true,
                RequiereColumnaExistente = true,
                InsertaSoloSiMontoMayorQueCero = false
            },
            new()
            {
                CodDeduccion = "C",
                Tipo = TipoCredito,
                ColumnasOrigen = ["APORTES"],
                RequiereCreditosHabilitados = true,
                InsertaSoloSiMontoMayorQueCero = true
            }
        ];
    }
}
