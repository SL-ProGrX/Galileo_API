using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Reflection;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndPrioridadesModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndPrioridadesDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmCoAplFndPrioridadesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de prioridades para aplicación de pagos.
        /// </summary>
        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<COAplFndPrioridadesListaResult>(ex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<COAplFndPrioridadesListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new COAplFndPrioridadesListaResult
                {
                    total = 0,
                    lista = new List<COAplFndPrioridadData>()
                }
            };

            try
            {
                var raw = conn.Query<dynamic>(CoAplFndPrioridadConst.SP_LISTA).AsList();

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
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<COAplFndPrioridadesListaResult>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de prioridades para aplicación de pagos.
        /// </summary>
        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_AplFnd_Prioridades_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Inserta o actualiza una prioridad según isNew y existencia del código.
        /// </summary>
        public ErrorDto Co_AplFnd_Prioridades_Guardar(int CodEmpresa, string usuario, COAplFndPrioridadData prioridad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (prioridad == null)
                    return DbHelper.ErrorResponse("Parámetros inválidos.", -2);

                string codigo = (prioridad.codigo ?? string.Empty).Trim().ToUpperInvariant();
                int orden = prioridad.orden;

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse("Indique un Código válido.", -2);

                if (orden < 0)
                    return DbHelper.ErrorResponse("Indique un Orden válido.", -2);

                bool existe = ExisteCodigo(conn, codigo);

                if (prioridad.isNew)
                {
                    if (existe)
                        return DbHelper.ErrorResponse($"El registro con el código {codigo} ya existe.", -2);

                    return Co_AplFnd_Prioridades_Persistir(conn, CodEmpresa, usuario, prioridad, orden, "REGISTRA - WEB");
                }

                if (!existe)
                    return DbHelper.ErrorResponse($"El registro con el código {codigo} no existe.", -2);

                return Co_AplFnd_Prioridades_Persistir(conn, CodEmpresa, usuario, prioridad, orden, "ACTUALIZA - WEB");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una prioridad por su código.
        /// </summary>
        public ErrorDto Co_AplFnd_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string cod = (codigo ?? string.Empty).Trim().ToUpperInvariant();

                var rs = conn.QueryFirstOrDefault<dynamic>(CoAplFndPrioridadConst.SP_DEL,new  {
                    Codigo = cod,
                    Usuario = usuario
                });

                int pass;
                string mensaje;
                string movimiento;
                LeerRespuestaSp(rs, out pass, out mensaje, out movimiento);

                if (pass != 1)
                    return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(mensaje) ? "No existe el registro a eliminar." : mensaje, -2);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Prioridad Apl. Pagos a Mora. Garantia: {cod}",
                    Movimiento = string.IsNullOrWhiteSpace(movimiento) ? "ELIMINA - WEB" : (movimiento.Trim() + " - WEB"),
                    Modulo = CoAplFndPrioridadConst.vModulo
                });

                return DbHelper.OkResponse(string.IsNullOrWhiteSpace(mensaje) ? "Eliminado satisfactoriamente." : mensaje);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las garantías disponibles para asignar prioridad.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    select
                        CODIGO as item,
                        DESCRIPCION as descripcion
                    from vCBR_Pagos_Apl_Config_Garantias_Disponibles
                    order by CODIGO;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene la prioridad actual de ejecución del proceso.
        /// </summary>
        public ErrorDto<int> Co_AplFnd_PrioridadEjecucion_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    select top 1 Valor
                    from CBR_ACUEDOS_APLICACION_PARAMETROS
                    where CODIGO = 'PRIO';";

                int? valor = conn.QueryFirstOrDefault<int?>(query);
                return valor ?? 0;
            });
        }

        /// <summary>
        /// Actualiza la prioridad de ejecución del proceso de aplicación de fondos.
        /// </summary>
        public ErrorDto Co_AplFnd_PrioridadEjecucion_Actualizar(int CodEmpresa, string usuario, int prioridad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (prioridad != 0 && prioridad != 1)
                    return DbHelper.ErrorResponse("La prioridad de ejecución es inválida.", -2);

                var p = new DynamicParameters();
                p.Add("@Valor", prioridad);
                p.Add("@Usuario", usuario);

                conn.Execute(CoAplFndPrioridadConst.SP_PRIORIDAD_EJECUCION, p);

                string detalleMovimiento = prioridad == 0
                    ? "Prioridad de Ejecución del Proceso de Aplicación de Fondos, Prioriza Pago a Creditos"
                    : "Prioridad de Ejecución del Proceso de Aplicación de Fondos, Prioriza Pago a Ahorro Obrero";

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = detalleMovimiento,
                    Movimiento = "CAMBIO - WEB",
                    Modulo = CoAplFndPrioridadConst.vModulo
                });

                return DbHelper.OkResponse(detalleMovimiento);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool ExisteCodigo(SqlConnection conn, string codigo)
        {
            var raw = conn.Query<dynamic>(CoAplFndPrioridadConst.SP_LISTA).AsList();

            for (int i = 0; i < raw.Count; i++)
            {
                string codRow = (raw[i]?.COD_GARANTIA ?? string.Empty).ToString().Trim().ToUpperInvariant();
                if (codRow == codigo)
                    return true;
            }

            return false;
        }

        private static List<COAplFndPrioridadData> MapRaw(List<dynamic> raw)
        {
            var lista = new List<COAplFndPrioridadData>();

            for (int i = 0; i < raw.Count; i++)
            {
                var r = raw[i];

                string codigo = (r?.COD_GARANTIA ?? string.Empty).ToString().Trim();
                string descripcion = (r?.DESCRIPCION ?? string.Empty).ToString().Trim();

                int orden = ParseInt(r?.ORDEN);
                int activoInt = ParseInt(r?.ACTIVO);

                string regFecha = (r?.REGISTRO_FECHA ?? string.Empty).ToString().Trim();
                string regUsuario = (r?.REGISTRO_USUARIO ?? string.Empty).ToString().Trim();
                string modFecha = (r?.MODIFICA_FECHA ?? string.Empty).ToString().Trim();
                string modUsuario = (r?.MODIFICA_USUARIO ?? string.Empty).ToString().Trim();

                lista.Add(new COAplFndPrioridadData
                {
                    codigo = codigo,
                    descripcion = descripcion,
                    orden = orden,
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
            if (string.IsNullOrWhiteSpace(v))
                v = "0";

            return Convert.ToInt32(v);
        }

        private static List<COAplFndPrioridadData> AplicarFiltro(List<COAplFndPrioridadData> lista, string? filtroIn)
        {
            string filtro = (filtroIn ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
                return lista;

            string qf = filtro.ToUpperInvariant();
            var filtrada = new List<COAplFndPrioridadData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var it = lista[i];
                string c = (it.codigo ?? string.Empty).Trim().ToUpperInvariant();
                string d = (it.descripcion ?? string.Empty).Trim().ToUpperInvariant();
                string o = Convert.ToString(it.orden).Trim().ToUpperInvariant();

                if (c.Contains(qf) || d.Contains(qf) || o.Contains(qf))
                    filtrada.Add(it);
            }

            return filtrada;
        }

        private static List<COAplFndPrioridadData> AplicarSort(List<COAplFndPrioridadData> lista, string? sortFieldIn, int sortOrder)
        {
            string sortField = (sortFieldIn ?? string.Empty).Trim() switch
            {
                CoAplFndPrioridadConst.SF_CODIGO => CoAplFndPrioridadConst.SF_CODIGO,
                CoAplFndPrioridadConst.SF_DESCRIPCION => CoAplFndPrioridadConst.SF_DESCRIPCION,
                CoAplFndPrioridadConst.SF_ORDEN => CoAplFndPrioridadConst.SF_ORDEN,
                CoAplFndPrioridadConst.SF_ACTIVO => CoAplFndPrioridadConst.SF_ACTIVO,
                CoAplFndPrioridadConst.SF_REGISTRO_FECHA => CoAplFndPrioridadConst.SF_REGISTRO_FECHA,
                CoAplFndPrioridadConst.SF_REGISTRO_USUARIO => CoAplFndPrioridadConst.SF_REGISTRO_USUARIO,
                CoAplFndPrioridadConst.SF_MODIFICA_FECHA => CoAplFndPrioridadConst.SF_MODIFICA_FECHA,
                CoAplFndPrioridadConst.SF_MODIFICA_USUARIO => CoAplFndPrioridadConst.SF_MODIFICA_USUARIO,
                _ => CoAplFndPrioridadConst.SF_ORDEN
            };

            bool desc = sortOrder == 0;

            lista.Sort((a, b) =>
            {
                int cmp = CompararCampo(a, b, sortField);
                return desc ? -cmp : cmp;
            });

            return lista;
        }

        private static int CompararCampo(COAplFndPrioridadData a, COAplFndPrioridadData b, string sortField)
        {
            return sortField switch
            {
                CoAplFndPrioridadConst.SF_CODIGO => string.Compare(a.codigo ?? string.Empty, b.codigo ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                CoAplFndPrioridadConst.SF_DESCRIPCION => string.Compare(a.descripcion ?? string.Empty, b.descripcion ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                CoAplFndPrioridadConst.SF_ORDEN => a.orden.CompareTo(b.orden),
                CoAplFndPrioridadConst.SF_ACTIVO => CompareBool(a.activo, b.activo),
                CoAplFndPrioridadConst.SF_REGISTRO_FECHA => string.Compare(a.registro_fecha ?? string.Empty, b.registro_fecha ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                CoAplFndPrioridadConst.SF_REGISTRO_USUARIO => string.Compare(a.registro_usuario ?? string.Empty, b.registro_usuario ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                CoAplFndPrioridadConst.SF_MODIFICA_FECHA => string.Compare(a.modifica_fecha ?? string.Empty, b.modifica_fecha ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                CoAplFndPrioridadConst.SF_MODIFICA_USUARIO => string.Compare(a.modifica_usuario ?? string.Empty, b.modifica_usuario ?? string.Empty, StringComparison.OrdinalIgnoreCase),
                _ => 0
            };
        }

        private static int CompareBool(bool a, bool b)
        {
            if (a == b)
                return 0;

            return a ? 1 : -1;
        }

        private static List<COAplFndPrioridadData> AplicarPaginacion(List<COAplFndPrioridadData> lista, int pagina, int paginacion)
        {
            var paged = new List<COAplFndPrioridadData>();

            int start = pagina < 0 ? 0 : pagina;
            int take = paginacion < 0 ? 0 : paginacion;

            int end = start + take;
            if (end > lista.Count)
                end = lista.Count;

            for (int i = start; i < end; i++)
                paged.Add(lista[i]);

            return paged;
        }

        /// <summary>
        /// Inserta o actualiza una prioridad y procesa la respuesta del SP.
        /// </summary>
        private ErrorDto Co_AplFnd_Prioridades_Persistir(
            SqlConnection conn,
            int CodEmpresa,
            string usuario,
            COAplFndPrioridadData prioridad,
            int orden,
            string movimientoDefault)
        {
            try
            {
                string codigo = (prioridad.codigo ?? string.Empty).Trim().ToUpperInvariant();
                int activo = prioridad.activo ? 1 : 0;

                var p = new DynamicParameters();
                p.Add("@Codigo", codigo);
                p.Add("@Orden", orden);
                p.Add("@Activo", activo);
                p.Add("@Usuario", usuario);

                var rs = conn.QueryFirstOrDefault<dynamic>(CoAplFndPrioridadConst.SP_ADD, p);

                return ProcesarRespuestaGuardar(
                    CodEmpresa,
                    usuario,
                    codigo,
                    rs,
                    movimientoDefault);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Procesa la respuesta de un SP de guardar y registra bitácora.
        /// </summary>
        private ErrorDto ProcesarRespuestaGuardar(int CodEmpresa, string usuario, string codigo, dynamic rs, string movimientoDefault)
        {
            int pass;
            string mensaje;
            string movimiento;
            LeerRespuestaSp(rs, out pass, out mensaje, out movimiento);

            if (pass != 1)
                return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(mensaje) ? "No fue posible guardar el registro." : mensaje, -2);

            DBBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                DetalleMovimiento = $"Prioridad Apl. Pagos a Mora. Garantia: {codigo}",
                Movimiento = string.IsNullOrWhiteSpace(movimiento) ? movimientoDefault : (movimiento.Trim() + " - WEB"),
                Modulo = CoAplFndPrioridadConst.vModulo
            });

            return DbHelper.OkResponse(string.IsNullOrWhiteSpace(mensaje) ? "Guardado satisfactoriamente." : mensaje);
        }

        /// <summary>
        /// Lee Pass/Mensaje/Movimiento desde el resultado dinámico de un SP.
        /// </summary>
        private static void LeerRespuestaSp(dynamic rs, out int pass, out string mensaje, out string movimiento)
        {
            string resultadoRaw = Convert.ToString(rs?.Pass ?? rs?.PASS ?? "0") ?? "0";
            string msgStr = Convert.ToString(rs?.Mensaje ?? rs?.MENSAJE ?? string.Empty) ?? string.Empty;
            string movStr = Convert.ToString(rs?.Movimiento ?? rs?.MOVIMIENTO ?? string.Empty) ?? string.Empty;

            pass = Convert.ToInt32(resultadoRaw.Trim() == string.Empty ? "0" : resultadoRaw.Trim());
            mensaje = msgStr;
            movimiento = movStr;
        }
    }
}
