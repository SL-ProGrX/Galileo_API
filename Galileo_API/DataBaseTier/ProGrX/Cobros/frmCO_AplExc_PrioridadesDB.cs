using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplExcPrioridadesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 4;

        // Constantes de sortField (evita S1192 y deja limpio)
        private const string SF_CODIGO = "codigo";
        private const string SF_DESCRIPCION = "descripcion";
        private const string SF_ORDEN = "orden";
        private const string SF_CONTRATO_APL = "contrato_apl";
        private const string SF_ACTIVO = "activo";
        private const string SF_REGISTRO_FECHA = "registro_fecha";
        private const string SF_REGISTRO_USUARIO = "registro_usuario";
        private const string SF_MODIFICA_FECHA = "modifica_fecha";
        private const string SF_MODIFICA_USUARIO = "modifica_usuario";

        public FrmCOAplExcPrioridadesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de prioridades para aplicación de excedentes a mora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<COAplExcPrioridadesListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new COAplExcPrioridadesListaResult
                {
                    total = 0,
                    lista = new List<COAplExcPrioridadData>()
                }
            };

            try
            {
                const string q = @"EXEC spCBR_Excedente_Apl_Config_Prioridades_Lista;";
                var raw = conn.Query<dynamic>(q).AsList();

                var lista = MapRaw(raw);
                lista = AplicarFiltro(lista, filtros.filtro);
                lista = AplicarSort(lista, filtros.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                bool exportAll = filtros.pagina == 0 || filtros.paginacion == 0;
                response.Result.lista = exportAll
                    ? lista
                    : AplicarPaginacion(lista, filtros.pagina, filtros.paginacion);

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<COAplExcPrioridadesListaResult>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de prioridades para aplicación de excedentes a mora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_AplExc_Prioridades_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Inserta o actualiza una prioridad según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="prioridad"></param>
        /// <returns></returns>
        public ErrorDto Co_AplExc_Prioridades_Guardar(int CodEmpresa, string usuario, COAplExcPrioridadData prioridad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (prioridad == null)
                    return DbHelper.ErrorResponse("Parámetros inválidos.", -2);

                string codigo = (prioridad.codigo ?? "").Trim().ToUpperInvariant();
                int orden = prioridad.orden;

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse("Indique un Código válido.", -2);

                if (orden < 0)
                    return DbHelper.ErrorResponse("Indique un Orden válido!", -2);

                bool existe = ExisteCodigo(conn, codigo);

                if (prioridad.isNew)
                {
                    if (existe)
                        return DbHelper.ErrorResponse($"El registro con el código {codigo} ya existe.", -2);

                    return Co_AplExc_Prioridades_Insertar(conn, CodEmpresa, usuario, prioridad, orden);
                }

                if (!existe)
                    return DbHelper.ErrorResponse($"El registro con el código {codigo} no existe.", -2);

                return Co_AplExc_Prioridades_Actualizar(conn, CodEmpresa, usuario, prioridad, orden);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una prioridad por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto Co_AplExc_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string cod = (codigo ?? "").Trim().ToUpperInvariant();

                var p = new DynamicParameters();
                p.Add("@Codigo", cod);
                p.Add("@Usuario", usuario);

                const string q = @"EXEC spCBR_Excedente_Apl_Config_Prioridades_Del @Codigo, @Usuario;";
                var rs = conn.QueryFirstOrDefault<dynamic>(q, p);

                int pass = Convert.ToInt32((rs?.Pass ?? rs?.PASS ?? 0).ToString());
                string mensaje = (rs?.Mensaje ?? rs?.MENSAJE ?? "").ToString();
                string movimiento = (rs?.Movimiento ?? rs?.MOVIMIENTO ?? "").ToString();

                if (pass != 1)
                    return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(mensaje) ? "No existe el registro a eliminar!" : mensaje, -2);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Prioridad Apl. Exc. a Mora. Garantia: {cod}",
                    Movimiento = string.IsNullOrWhiteSpace(movimiento) ? "ELIMINA - WEB" : (movimiento.Trim() + " - WEB"),
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(string.IsNullOrWhiteSpace(mensaje) ? "Eliminado satisfactoriamente." : mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las garantías disponibles para asignar prioridad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplExc_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    select
                        CODIGO as item,
                        DESCRIPCION as descripcion
                    from vCBR_Excedente_Apl_Config_Garantias_Disponibles
                    order by CODIGO;";
                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        private static bool ExisteCodigo(SqlConnection conn, string codigo)
        {
            const string qLista = @"EXEC spCBR_Excedente_Apl_Config_Prioridades_Lista;";
            var raw = conn.Query<dynamic>(qLista).AsList();

            for (int i = 0; i < raw.Count; i++)
            {
                string codRow = (raw[i]?.COD_GARANTIA ?? "").ToString().Trim().ToUpperInvariant();
                if (codRow == codigo) return true;
            }

            return false;
        }

        private static List<COAplExcPrioridadData> MapRaw(List<dynamic> raw)
        {
            var lista = new List<COAplExcPrioridadData>();

            for (int i = 0; i < raw.Count; i++)
            {
                var r = raw[i];

                string codigo = (r?.COD_GARANTIA ?? "").ToString().Trim();
                string descripcion = (r?.DESCRIPCION ?? "").ToString().Trim();

                int orden = ParseInt(r?.ORDEN);
                int contratoAplInt = ParseInt(r?.APLICA_CONTRATO);
                int activoInt = ParseInt(r?.ACTIVO);

                string regFecha = (r?.REGISTRO_FECHA ?? "").ToString().Trim();
                string regUsuario = (r?.REGISTRO_USUARIO ?? "").ToString().Trim();
                string modFecha = (r?.MODIFICA_FECHA ?? "").ToString().Trim();
                string modUsuario = (r?.MODIFICA_USUARIO ?? "").ToString().Trim();

                lista.Add(new COAplExcPrioridadData
                {
                    codigo = codigo,
                    descripcion = descripcion,
                    orden = orden,
                    contrato_apl = contratoAplInt == 1,
                    activo = activoInt == 1,
                    registro_fecha = regFecha,
                    registro_usuario = regUsuario,
                    modifica_fecha = modFecha,
                    modifica_usuario = modUsuario,
                    isNew = false
                });
            }

            return lista;
        }

        private static int ParseInt(object? value)
        {
            string v = (value ?? 0).ToString()!.Trim();
            if (string.IsNullOrWhiteSpace(v)) v = "0";
            return Convert.ToInt32(v);
        }

        private static List<COAplExcPrioridadData> AplicarFiltro(List<COAplExcPrioridadData> lista, string? filtroIn)
        {
            string filtro = (filtroIn ?? "").Trim();
            if (string.IsNullOrWhiteSpace(filtro)) return lista;

            string qf = filtro.ToUpperInvariant();
            var filtrada = new List<COAplExcPrioridadData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var it = lista[i];
                string c = (it.codigo ?? "").Trim().ToUpperInvariant();
                string d = (it.descripcion ?? "").Trim().ToUpperInvariant();
                string o = Convert.ToString(it.orden).Trim().ToUpperInvariant();

                if (c.Contains(qf) || d.Contains(qf) || o.Contains(qf))
                    filtrada.Add(it);
            }

            return filtrada;
        }

        private static List<COAplExcPrioridadData> AplicarSort(List<COAplExcPrioridadData> lista, string? sortFieldIn, int sortOrder)
        {
            string sortField = (sortFieldIn ?? "").Trim() switch
            {
                SF_CODIGO => SF_CODIGO,
                SF_DESCRIPCION => SF_DESCRIPCION,
                SF_ORDEN => SF_ORDEN,
                SF_CONTRATO_APL => SF_CONTRATO_APL,
                SF_ACTIVO => SF_ACTIVO,
                SF_REGISTRO_FECHA => SF_REGISTRO_FECHA,
                SF_REGISTRO_USUARIO => SF_REGISTRO_USUARIO,
                SF_MODIFICA_FECHA => SF_MODIFICA_FECHA,
                SF_MODIFICA_USUARIO => SF_MODIFICA_USUARIO,
                _ => SF_ORDEN
            };

            bool desc = sortOrder == 0;

            lista.Sort((a, b) =>
            {
                int cmp = CompararCampo(a, b, sortField);
                return desc ? -cmp : cmp;
            });

            return lista;
        }

        private static int CompararCampo(COAplExcPrioridadData a, COAplExcPrioridadData b, string sortField)
        {
            return sortField switch
            {
                SF_CODIGO => string.Compare(a.codigo ?? "", b.codigo ?? "", StringComparison.OrdinalIgnoreCase),
                SF_DESCRIPCION => string.Compare(a.descripcion ?? "", b.descripcion ?? "", StringComparison.OrdinalIgnoreCase),
                SF_ORDEN => a.orden.CompareTo(b.orden),
                SF_CONTRATO_APL => CompareBool(a.contrato_apl, b.contrato_apl),
                SF_ACTIVO => CompareBool(a.activo, b.activo),
                SF_REGISTRO_FECHA => string.Compare(a.registro_fecha ?? "", b.registro_fecha ?? "", StringComparison.OrdinalIgnoreCase),
                SF_REGISTRO_USUARIO => string.Compare(a.registro_usuario ?? "", b.registro_usuario ?? "", StringComparison.OrdinalIgnoreCase),
                SF_MODIFICA_FECHA => string.Compare(a.modifica_fecha ?? "", b.modifica_fecha ?? "", StringComparison.OrdinalIgnoreCase),
                SF_MODIFICA_USUARIO => string.Compare(a.modifica_usuario ?? "", b.modifica_usuario ?? "", StringComparison.OrdinalIgnoreCase),
                _ => 0
            };
        }

        private static int CompareBool(bool a, bool b)
        {
            if (a == b) return 0;
            return a ? 1 : -1;
        }

        private static List<COAplExcPrioridadData> AplicarPaginacion(List<COAplExcPrioridadData> lista, int pagina, int paginacion)
        {
            var paged = new List<COAplExcPrioridadData>();

            int start = pagina < 0 ? 0 : pagina;
            int take = paginacion < 0 ? 0 : paginacion;

            int end = start + take;
            if (end > lista.Count) end = lista.Count;

            for (int i = start; i < end; i++)
                paged.Add(lista[i]);

            return paged;
        }

        /// <summary>
        /// Inserta una prioridad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="prioridad"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        private ErrorDto Co_AplExc_Prioridades_Insertar(SqlConnection conn, int CodEmpresa, string usuario, COAplExcPrioridadData prioridad, int orden)
        {
            try
            {
                string codigo = (prioridad.codigo ?? "").Trim().ToUpperInvariant();
                int cntAplica = prioridad.contrato_apl ? 1 : 0;
                int activo = prioridad.activo ? 1 : 0;

                var p = new DynamicParameters();
                p.Add("@Codigo", codigo);
                p.Add("@Orden", orden);
                p.Add("@CntAplica", cntAplica);
                p.Add("@Activo", activo);
                p.Add("@Usuario", usuario);

                const string q = @"EXEC spCBR_Excedente_Apl_Config_Prioridades_Add @Codigo, @Orden, @CntAplica, @Activo, @Usuario;";
                var rs = conn.QueryFirstOrDefault<dynamic>(q, p);

                int pass = Convert.ToInt32((rs?.Pass ?? rs?.PASS ?? 0).ToString());
                string mensaje = (rs?.Mensaje ?? rs?.MENSAJE ?? "").ToString();
                string movimiento = (rs?.Movimiento ?? rs?.MOVIMIENTO ?? "").ToString();

                if (pass != 1)
                    return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(mensaje) ? "No fue posible guardar el registro." : mensaje, -2);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Prioridad Apl. Exc. a Mora. Garantia: {codigo}",
                    Movimiento = string.IsNullOrWhiteSpace(movimiento) ? "REGISTRA - WEB" : (movimiento.Trim() + " - WEB"),
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(string.IsNullOrWhiteSpace(mensaje) ? "Guardado satisfactoriamente." : mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una prioridad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="prioridad"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        private ErrorDto Co_AplExc_Prioridades_Actualizar(SqlConnection conn, int CodEmpresa, string usuario, COAplExcPrioridadData prioridad, int orden)
        {
            return Co_AplExc_Prioridades_Insertar(conn, CodEmpresa, usuario, prioridad, orden);
        }
    }
}
