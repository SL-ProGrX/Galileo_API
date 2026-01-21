using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesChequesBancoPuenteDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmTesChequesBancoPuenteDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mTesoreria = new MTesoreria(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosGestion_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select id_banco as item,descripcion from tes_bancos where estado = 'A' and puente  = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        private ErrorDto<string> TES_CuentaBanco(int CodEmpresa, int id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select ctaconta as Cuenta from tes_bancos where id_banco = @banco ";

                return conn.Query<string>(query,
                        new { banco = id_banco }
                        ).FirstOrDefault() ?? string.Empty;
            });
        }

        public ErrorDto TES_ChequesBanco_Aplica(int CodEmpresa, int id_banco, int banco, string usuario ,List<ChequesBancoPuenteData> data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                if (id_banco == banco)
                {
                    return DbHelper.ErrorResponse("- Banco de traslado es igual al banco puente ");
                }

                string vCuentaI = TES_CuentaBanco(CodEmpresa, id_banco).Result ?? string.Empty;
                string vCuentaD = TES_CuentaBanco(CodEmpresa, banco).Result ?? string.Empty;

                var solicitudes = data.Select(x => x.nsolicitud).ToList();

                var estados = conn.Query<(int nsolicitud, string estado_asiento)>(
                        @"SELECT nsolicitud, estado_asiento 
                          FROM cheques 
                          WHERE nsolicitud IN @solicitudes;",
                        new { solicitudes }
                    ).ToList();

                if (estados.Any(e => e.estado_asiento == "G"))
                {
                    return DbHelper.ErrorResponse("El asiento de esta solicitud ya fue generado, no se puede reclasificar...");
                }

                data.Select(x => x.nsolicitud).ToList().ForEach(nsolicitud =>
                {
                    var updateQuery = $@"update cheques set 
                                    id_banco = @banco
                                    where nsolicitud = @solicitud ";
                    conn.Execute(updateQuery, new { banco = banco, solicitud = nsolicitud });

                    updateQuery = $@"update ck_detalle set 
                                cuenta_contable = @cuentaD
                                where cuenta_contable = @cuentaI 
                                and nsolicitud = @solicitud ";

                    conn.Execute(updateQuery, new { cuentaD = vCuentaD, cuentaI = vCuentaI, solicitud = nsolicitud });

                    //Bitácora
                    _Security_MainDB.Bitacora
                        (new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Cambia de Banco Solicitud N. {nsolicitud} ",
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
                        });
                });

                return DbHelper.OkResponse("Proceso realizado con éxito.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto<List<ChequesBancoPuenteData>> TES_ChequePuenteLista_Obtener(int CodEmpresa, int id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select 0 as control ,nsolicitud,codigo,beneficiario,monto,fecha_solicitud 
                                    from cheques 
                                    where id_banco = @banco and 
                                    ESTADO = 'P' and tipo = 'CK'";

                return conn.Query<ChequesBancoPuenteData>(query,
                        new
                        {
                            banco = id_banco
                        }).ToList();
            });
        }
    }
}
