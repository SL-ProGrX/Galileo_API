using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaAsociadosDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizaAsociadosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Lista de pólizas para cargar combo (VB6: vPoliza_Catalogo Where Tipo='PA').
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_AsociadoCatalogo_Listar(int CodEmpresa, string tipo = "PA")
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                SELECT
                    COD_POLIZA   AS item,
                    RTRIM(Poliza_Desc) AS descripcion
                FROM vPoliza_Catalogo
                WHERE Tipo = @Tipo
                ORDER BY COD_POLIZA";

                return conn.Query<DropDownListaGenericaModel>(query, new { Tipo = (tipo ?? "PA").Trim() }).ToList();
            });
        }


        /// <summary>
        /// Asociados por corte (VB6: exec spPoliza_Asociados 'yyyy/MM/dd', Usuario, Tipo)
        /// Tipos esperados: 'T' | 'I' | 'E' | 'SC'
        /// </summary>
        public ErrorDto<List<PolizaAsociadoCorteSpDto>> Poliza_Asociados_Corte_Listar(
        int CodEmpresa,
        string Usuario,
        DateTime FechaCorte,
        string? Tipo)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var tipo = (Tipo ?? "T").Trim().ToUpperInvariant();
                if (tipo is not ("T" or "I" or "E" or "SC"))
                    throw new ArgumentException("Tipo inválido. Use T, I, E o SC.");

                const string execSp = @"EXEC spPoliza_Asociados @Corte, @Usuario, @Movimiento;";

                return conn.Query<PolizaAsociadoCorteSpDto>(execSp, new
                {
                    Corte = FechaCorte.Date,
                    Usuario = (Usuario ?? "").Trim(),
                    Movimiento = tipo
                }).ToList();
            });
        }


        /// <summary>
        /// Beneficiarios por póliza (VB6: exec spPoliza_Beneficiarios_Lista '@CodPoliza')
        /// </summary>
        public ErrorDto<List<PolizaBeneficiariosSpDto>> Poliza_Beneficiarios_Listar(
            int CodEmpresa,
            string CodPoliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(CodPoliza))
                    throw new ArgumentException("Código de póliza requerido.");

                const string execSp = @"EXEC spPoliza_Beneficiarios_Lista @Poliza;";

                return conn.Query<PolizaBeneficiariosSpDto>(execSp, new
                {
                    Poliza = CodPoliza.Trim()
                }).ToList();
            });
        }

    }
}
