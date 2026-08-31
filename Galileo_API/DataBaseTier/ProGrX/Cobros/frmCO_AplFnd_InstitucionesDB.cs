using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplFndInstitucionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 4;

        public FrmCOAplFndInstitucionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Lista de instituciones
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoAplFndInstitucionesListaResult> CoAplFndInstitucionesListaObtener(int CodEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<CoAplFndInstitucionesListaResult>(
                    "Usuario de sesión inválido.",
                    -2
                );
            }

            var pUsuario = usuario.Trim();

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Usuario", pUsuario, DbType.String);
                var rows = conn.Query(
                    "dbo.spCBR_Instituciones_Planilla_Lista",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                );
                static string ToTrimString(object? v) => (Convert.ToString(v) ?? string.Empty).Trim();

                static int ToInt(object? v)
                {
                    var s = Convert.ToString(v);
                    return string.IsNullOrWhiteSpace(s) ? 0 : Convert.ToInt32(s);
                }

                static string ToFechaCorte(object? v)
                {
                    if (v == null) return string.Empty;
                    if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");

                    var s = Convert.ToString(v);
                    return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();
                }

                static bool ToBool01(object? v)
                {
                    if (v == null) return false;
                    var s = Convert.ToString(v);
                    if (string.IsNullOrWhiteSpace(s)) return false;
                    return Convert.ToInt32(s) == 1;
                }

                static CoAplFndInstitucionesData Map(dynamic r)
                {
                    return new CoAplFndInstitucionesData
                    {
                        cod_institucion = ToInt((object?)r?.COD_INSTITUCION),
                        descripcion = ToTrimString((object?)r?.DESCRIPCION),
                        fecha_corte = ToFechaCorte((object?)r?.PR_FECHA_CORTE),
                        aplica_pagos = ToBool01((object?)r?.IND_APLICA_PAGOS),
                        isNew = false
                    };
                }

                var lista = new List<CoAplFndInstitucionesData>();
                foreach (var r in rows)
                {
                    lista.Add(Map(r));
                }

                return new CoAplFndInstitucionesListaResult
                {
                    total = lista.Count,
                    lista = lista
                };
            });
        }



        /// <summary>
        /// Actualiza indicador de una institución
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Co_AplFnd_Instituciones_Actualizar(int CodEmpresa, CoAplFndInstitucionesActualizarRequest req)
        {
            if (req == null || req.cod_institucion <= 0)
            {
                return DbHelper.ErrorResponse("Institución inválida.", -2);
            }

            if (string.IsNullOrWhiteSpace(req.usuario_sesion))
            {
                return DbHelper.ErrorResponse("Usuario de sesión inválido.", -2);
            }

            var p = new DynamicParameters();
            p.Add("@CodInstitucion", req.cod_institucion, DbType.Int32);
            p.Add("@IndAplicaPago", req.aplica_pagos ? 1 : 0, DbType.Int32);

            var exec = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                "dbo.spCBR_Instituciones_Planilla_Update",
                p);

            if (exec.Code != 0)
                return exec;

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = req.usuario_sesion.Trim(),
                DetalleMovimiento =
                    $"Cobros > Planilla > Institución {req.cod_institucion}, AplicaPagos={req.aplica_pagos}",
                Movimiento = "Modifica - WEB",
                Modulo = vModulo
            });

            return DbHelper.CreateOkResponse();
        }
    }
}
