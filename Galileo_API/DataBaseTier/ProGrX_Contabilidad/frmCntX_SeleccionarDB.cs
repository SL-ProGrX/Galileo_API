using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXSeleccionarDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCntXModuloDb _cntXModuloDb;

        public FrmCntXSeleccionarDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntXModuloDb = new MCntXModuloDb(config);
        }

        /// <summary>
        /// Carga el estado inicial para frmCntX_Seleccionar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="muestraTodas"></param>
        /// <returns></returns>
        public ErrorDto<CntXSeleccionarCargaResponse> CntX_Seleccionar_CargaInicial(int codEmpresa, string usuario, bool muestraTodas)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                ValidarUsuario(usuario);

                int totalContabilidades = conn.ExecuteScalar<int>(
                    "select isnull(count(1), 0) from CntX_Contabilidades");

                if (totalContabilidades == 0)
                {
                    return new CntXSeleccionarCargaResponse
                    {
                        requiereCrearContabilidad = true,
                        mensaje = "No Existen Contabilidades Creadas, Necesita Crear al menos Una..."
                    };
                }

                var response = new CntXSeleccionarCargaResponse
                {
                    contabilidades = ObtenerContabilidadesUsuario(conn, usuario, string.Empty)
                };

                if (!muestraTodas)
                {
                    int codHistorico = ObtenerHistoricoUsuario(conn, usuario);
                    if (codHistorico > 0)
                    {
                        response.contabilidadSeleccionada = SeleccionarContabilidad(codEmpresa, usuario, codHistorico);
                    }
                }

                if (muestraTodas && response.contabilidades.Count == 1)
                {
                    response.contabilidadSeleccionada = SeleccionarContabilidad(
                        codEmpresa,
                        usuario,
                        response.contabilidades[0].cod_contabilidad);
                }

                return response;
            });
        }

        /// <summary>
        /// Busca contabilidades disponibles para frmCntX_Seleccionar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXSeleccionarContabilidadItem>> CntX_Seleccionar_Buscar(int codEmpresa, string usuario, string filtro)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                ValidarUsuario(usuario);
                return ObtenerContabilidadesUsuario(conn, usuario, filtro);
            });
        }

        /// <summary>
        /// Selecciona una contabilidad para frmCntX_Seleccionar y guarda el historico del usuario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CntXParametrosDto> CntX_Seleccionar_Seleccionar(int codEmpresa, string usuario, int codContabilidad)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<CntXParametrosDto>("El codigo de contabilidad es requerido.");
            }

            var acceso = ValidarAccesoContabilidad(codEmpresa, usuario, codContabilidad);
            if (acceso.Code < 0)
            {
                return DbHelper.CreateErrorResponse<CntXParametrosDto>(acceso.Description ?? "No fue posible validar el acceso.");
            }

            var parametros = _cntXModuloDb.sbCntX_Contabilidad_Selecciona(codEmpresa, codContabilidad);
            if (parametros.Code < 0 || parametros.Result == null)
            {
                return DbHelper.CreateErrorResponse<CntXParametrosDto>(parametros.Description ?? "No fue posible seleccionar la contabilidad.");
            }

            var historico = _cntXModuloDb.sbCntX_Estado_Guarda(
                codEmpresa,
                usuario,
                parametros.Result.CodigoConta,
                parametros.Result.PeriodoAnio,
                parametros.Result.PeriodoMes);

            if (historico.Code < 0)
            {
                return DbHelper.CreateErrorResponse<CntXParametrosDto>(historico.Description ?? "No fue posible guardar el historico de acceso.");
            }

            return DbHelper.CreateOkResponse(parametros.Result);
        }

        private ErrorDto<bool> ValidarAccesoContabilidad(int codEmpresa, string usuario, int codContabilidad)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                ValidarUsuario(usuario);

                const string sql = @"
                    select count(1)
                    from CntX_Contabilidades C
                    inner join CNTX_CONTA_USUARIOS U
                      on C.cod_contabilidad = U.cod_contabilidad
                     and U.usuario = @Usuario
                    where C.cod_contabilidad = @CodContabilidad";

                bool tieneAcceso = conn.ExecuteScalar<int>(sql, new { Usuario = usuario, CodContabilidad = codContabilidad }) > 0;
                if (!tieneAcceso)
                {
                    throw new InvalidOperationException("Esta Contabilidad a sido Eliminada o no tiene Acceso a ella, verifique...");
                }

                return true;
            });
        }

        private static List<CntXSeleccionarContabilidadItem> ObtenerContabilidadesUsuario(
            SqlConnection conn,
            string usuario,
            string filtro)
        {
            const string sql = @"
                select C.cod_contabilidad,
                       rtrim(C.nombre) as nombre
                from CntX_Contabilidades C
                inner join CNTX_CONTA_USUARIOS U
                  on C.cod_contabilidad = U.cod_contabilidad
                 and U.usuario = @Usuario
                where (@Filtro = '' or C.nombre like '%' + @Filtro + '%')
                order by C.cod_contabilidad";

            return conn.Query<CntXSeleccionarContabilidadItem>(
                sql,
                new { Usuario = usuario, Filtro = (filtro ?? string.Empty).Trim() }).ToList();
        }

        private static int ObtenerHistoricoUsuario(SqlConnection conn, string usuario)
        {
            const string sql = @"
                select top 1 cod_contabilidad
                from CntX_Acceso_Historico
                where usuario = @Usuario";

            return conn.QueryFirstOrDefault<int?>(sql, new { Usuario = usuario }) ?? 0;
        }

        private CntXParametrosDto? SeleccionarContabilidad(int codEmpresa, string usuario, int codContabilidad)
        {
            var result = CntX_Seleccionar_Seleccionar(codEmpresa, usuario, codContabilidad);
            return result.Code < 0 ? null : result.Result;
        }

        private static void ValidarUsuario(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new InvalidOperationException("El usuario es requerido.");
            }
        }
    }
}
