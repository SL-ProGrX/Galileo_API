using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_ComisionesAutorizaDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_ComisionesAutorizaDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de socios para autorizar comisiones, aplicando filtros opcionales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<ComisionAutorizaData>> AF_ComisionesAutoriza_Obtener(int CodEmpresa, ComisionAutorizaFiltroDto filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ComisionAutorizaData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ComisionAutorizaData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var filtros = "";
                    filtros += " AND S.estadoactual = 'S' AND S.Fecha_Comision IS NULL ";
                    filtros += " AND S.fechaIngreso BETWEEN @inicio AND @corte ";
                    filtros += " AND P.apl_comision = 1 ";

                    if (filtro.ChkAportes)
                        filtros += " AND dbo.fxAFIComisionAporte(S.FechaIngreso, S.Cedula) > 0 ";

                    if (filtro.ChkPromotor)
                        filtros += " AND S.id_promotor = @idPromotor ";

                    if (filtro.ChkUsuarios)
                        filtros += " AND S.reg_user = @usuario ";

                    if (filtro.Autorizado.HasValue)
                    {
                        if (filtro.Autorizado == 1 || filtro.Autorizado == 2)
                            filtros += " AND S.Comision_Autoriza = @autorizado ";
                        else if (filtro.Autorizado == 0)
                            filtros += " AND ISNULL(S.Comision_Autoriza,0) = 0 ";
                    }

                    var query = $@"SELECT
                                    S.id_Boleta_AF AS IdBoleta,
                                    S.Cedula,
                                    S.Nombre,
                                    S.id_promotor AS IdPromotor,
                                    ISNULL(S.Comision_Autoriza, 0) AS AutorizacionX,
                                    S.Comision_Autoriza,
                                    S.FechaIngreso,
                                    S.EstadoActual,
                                    S.reg_Fecha AS Fecha_Comision,
                                    S.reg_user AS Reg_User,
                                    S.AUTORIZA_COMISION_NOTAS AS Autoriza_Comision_Notas,
                                    P.Nombre AS PromotorX
                                FROM socios S
                                INNER JOIN promotores P ON S.id_promotor = P.id_promotor
                                {filtros}
                                ORDER BY S.FechaIngreso";

                    result.Result = connection.Query<ComisionAutorizaData>(query, new
                    {
                        inicio = filtro.Inicio.Date,
                        corte = filtro.Corte.Date.AddDays(1).AddSeconds(-1),
                        idPromotor = filtro.IdPromotor,
                        usuario = filtro.Usuario,
                        autorizado = filtro.Autorizado
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Actualiza la autorización de comisión de un socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="autoriza"></param>
        /// <param name="notas"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesAutoriza_Autorizar(int CodEmpresa, string cedula, int autoriza, string? notas, string usuario)
        {
            string notasValida = notas ?? "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"UPDATE socios
                                  SET Comision_Autoriza = @autoriza,
                                      AUTORIZA_COMISION_NOTAS = @notasValida
                                  WHERE cedula = @cedula";
                    connection.Execute(query, new
                    {
                        autoriza,
                        notasValida = notasValida,
                        cedula
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Comisión Autoriza: {cedula} - {notas}",
                        Movimiento = autoriza == 1 ? "Autoriza Comisión - WEB" : "Desautoriza Comisión - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
