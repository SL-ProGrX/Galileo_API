using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOCausasYArreglosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 4;
        private const string SF_CAUSA_COD = "cod_causa";
        private const string SF_CAUSA_DESC = "descripcion";
        private const string SF_CAUSA_ACTIVA = "activa";
        private const string SF_ARREGLO_COD = "cod_arreglo";
        private const string SF_ARREGLO_DESC = "descripcion";
        private const string SF_ARREGLO_ACTIVO = "activo";
        private const string MSG_GUARDADO_OK = "Guardado satisfactoriamente.";
        private const string MSG_ELIMINADO_OK = "Eliminado satisfactoriamente.";
        private const string MSG_ACTUALIZADO_OK = "Actualizado satisfactoriamente.";


        public FrmCOCausasYArreglosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de Causas de Morosidad (CBR_CAUSAS_MOROSIDAD) con lazyload (paginación en memoria).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<COCausaMorosidadListaResult>(ex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<COCausaMorosidadListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new COCausaMorosidadListaResult
                {
                    total = 0,
                    lista = new List<COCausaMorosidadData>()
                }
            };

            try
            {
                const string q = @"
                    SELECT
                        RTRIM(COD_CAUSA) AS cod_causa,
                        RTRIM(DESCRIPCION) AS descripcion,
                        CASE WHEN ISNULL(ACTIVA,1) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS activa,
                        CAST(0 AS bit) AS isNew
                    FROM dbo.CBR_CAUSAS_MOROSIDAD;";

                var lista = conn.Query<COCausaMorosidadData>(q).ToList();

                lista = AplicarFiltroCausas(lista, filtros.filtro);
                lista = AplicarSortCausas(lista, filtros.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                bool exportAll = filtros.pagina == 0 || filtros.paginacion == 0;
                response.Result.lista = exportAll
                    ? lista
                    : AplicarPaginacion(lista, filtros.pagina, filtros.paginacion);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<COCausaMorosidadListaResult>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de Causas de Morosidad (CBR_CAUSAS_MOROSIDAD).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_CausasMorosidad_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Inserta o actualiza una Causa de Morosidad según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        public ErrorDto Co_CausasMorosidad_Guardar(int CodEmpresa, string usuario, COCausaMorosidadData causa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (causa == null)
                    return DbHelper.ErrorResponse("Parámetros inválidos.", -2);

                string cod = (causa.cod_causa ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de causa inválido.", -2);

                bool existe = ExisteCausa(conn, cod);

                if (causa.isNew)
                {
                    if (existe)
                        return DbHelper.ErrorResponse($"La Causa con el código {cod} ya existe.", -2);

                    return Co_CausasMorosidad_Insertar(conn, CodEmpresa, usuario, causa);
                }

                if (!existe)
                    return DbHelper.ErrorResponse($"La Causa con el código {cod} no existe.", -2);

                return Co_CausasMorosidad_Actualizar(conn, CodEmpresa, usuario, causa);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una Causa de Morosidad por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_causa"></param>
        /// <returns></returns>
        public ErrorDto Co_CausasMorosidad_Eliminar(int CodEmpresa, string usuario, string cod_causa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string cod = (cod_causa ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de causa inválido.", -2);

                const string q = @"DELETE FROM dbo.CBR_CAUSAS_MOROSIDAD WHERE UPPER(RTRIM(COD_CAUSA)) = @cod;";
                var rows = conn.Execute(q, new { cod });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No existe el registro a eliminar.", -2);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Causa de Morosidad: {cod}",
                    Movimiento = "ELIMINA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_ELIMINADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de Tipos de Arreglos (CBR_TIPOS_ARREGLOS) con lazyload (paginación en memoria).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<COArregloPagoTipoListaResult>(ex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<COArregloPagoTipoListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new COArregloPagoTipoListaResult
                {
                    total = 0,
                    lista = new List<COArregloPagoTipoData>()
                }
            };

            try
            {
                const string q = @"
                    SELECT
                        RTRIM(COD_ARREGLO) AS cod_arreglo,
                        RTRIM(DESCRIPCION) AS descripcion,
                        CASE WHEN ISNULL(ACTIVO,1) = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS activo,
                        CAST(0 AS bit) AS isNew
                    FROM dbo.CBR_TIPOS_ARREGLOS;";

                var lista = conn.Query<COArregloPagoTipoData>(q).ToList();

                lista = AplicarFiltroArreglos(lista, filtros.filtro);
                lista = AplicarSortArreglos(lista, filtros.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                bool exportAll = filtros.pagina == 0 || filtros.paginacion == 0;
                response.Result.lista = exportAll
                    ? lista
                    : AplicarPaginacion(lista, filtros.pagina, filtros.paginacion);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<COArregloPagoTipoListaResult>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de Tipos de Arreglos (CBR_TIPOS_ARREGLOS).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_TiposArreglos_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Inserta o actualiza un Tipo de Arreglo según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto Co_TiposArreglos_Guardar(int CodEmpresa, string usuario, COArregloPagoTipoData tipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (tipo == null)
                    return DbHelper.ErrorResponse("Parámetros inválidos.", -2);

                string cod = (tipo.cod_arreglo ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de arreglo inválido.", -2);

                bool existe = ExisteArreglo(conn, cod);

                if (tipo.isNew)
                {
                    if (existe)
                        return DbHelper.ErrorResponse($"El Tipo de Arreglo con el código {cod} ya existe.", -2);

                    return Co_TiposArreglos_Insertar(conn, CodEmpresa, usuario, tipo);
                }

                if (!existe)
                    return DbHelper.ErrorResponse($"El Tipo de Arreglo con el código {cod} no existe.", -2);

                return Co_TiposArreglos_Actualizar(conn, CodEmpresa, usuario, tipo);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un Tipo de Arreglo por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_arreglo"></param>
        /// <returns></returns>
        public ErrorDto Co_TiposArreglos_Eliminar(int CodEmpresa, string usuario, string cod_arreglo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string cod = (cod_arreglo ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de arreglo inválido.", -2);

                const string q = @"DELETE FROM dbo.CBR_TIPOS_ARREGLOS WHERE UPPER(RTRIM(COD_ARREGLO)) = @cod;";
                var rows = conn.Execute(q, new { cod });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No existe el registro a eliminar.", -2);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Tipo de Arreglo: {cod}",
                    Movimiento = "ELIMINA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_ELIMINADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta una Causa de Morosidad.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto Co_CausasMorosidad_Insertar(SqlConnection conn, int CodEmpresa, string usuario, COCausaMorosidadData causa)
        {
            try
            {
                string cod = (causa.cod_causa ?? "").Trim().ToUpperInvariant();
                string desc = (causa.descripcion ?? "").Trim().ToUpperInvariant();
                int activa = causa.activa ? 1 : 0;
                string user = (usuario ?? "").Trim();

                const string q = @"
                    INSERT INTO dbo.CBR_CAUSAS_MOROSIDAD
                    (
                        COD_CAUSA,
                        DESCRIPCION,
                        ACTIVA,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @cod,
                        @desc,
                        @activa,
                        @user,
                        dbo.MyGetdate()
                    );";

                conn.Execute(q, new { cod, desc, activa, user });

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = user.ToUpperInvariant(),
                    DetalleMovimiento = $"Causa de Morosidad: {cod} - {desc}",
                    Movimiento = "REGISTRA - WEB",
                    Modulo = vModulo
                });
                return DbHelper.OkResponse(MSG_GUARDADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una Causa de Morosidad.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto Co_CausasMorosidad_Actualizar(SqlConnection conn, int CodEmpresa, string usuario, COCausaMorosidadData causa)
        {
            try
            {
                string cod = (causa.cod_causa ?? "").Trim().ToUpperInvariant();
                string desc = (causa.descripcion ?? "").Trim().ToUpperInvariant();
                int activa = causa.activa ? 1 : 0;

                const string q = @"
                    UPDATE dbo.CBR_CAUSAS_MOROSIDAD
                    SET
                        DESCRIPCION = @desc,
                        ACTIVA      = @activa
                    WHERE UPPER(RTRIM(COD_CAUSA)) = @cod;";

                conn.Execute(q, new { cod, desc, activa });

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Causa de Morosidad: {cod} - {desc}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_ACTUALIZADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un Tipo de Arreglo.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Co_TiposArreglos_Insertar(SqlConnection conn, int CodEmpresa, string usuario, COArregloPagoTipoData tipo)
        {
            try
            {
                string cod = (tipo.cod_arreglo ?? "").Trim().ToUpperInvariant();
                string desc = (tipo.descripcion ?? "").Trim().ToUpperInvariant();
                int activo = tipo.activo ? 1 : 0;
                string user = (usuario ?? "").Trim();

                const string q = @"
                    INSERT INTO dbo.CBR_TIPOS_ARREGLOS
                    (
                        COD_ARREGLO,
                        DESCRIPCION,
                        ACTIVO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @cod,
                        @desc,
                        @activo,
                        @user,
                        dbo.MyGetdate()
                    );";

                conn.Execute(q, new { cod, desc, activo, user });

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = user.ToUpperInvariant(),
                    DetalleMovimiento = $"Tipo de Arreglo: {cod} - {desc}",
                    Movimiento = "REGISTRA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_GUARDADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un Tipo de Arreglo.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Co_TiposArreglos_Actualizar(SqlConnection conn, int CodEmpresa, string usuario, COArregloPagoTipoData tipo)
        {
            try
            {
                string cod = (tipo.cod_arreglo ?? "").Trim().ToUpperInvariant();
                string desc = (tipo.descripcion ?? "").Trim().ToUpperInvariant();
                int activo = tipo.activo ? 1 : 0;

                const string q = @"
                    UPDATE dbo.CBR_TIPOS_ARREGLOS
                    SET
                        DESCRIPCION = @desc,
                        ACTIVO      = @activo
                    WHERE UPPER(RTRIM(COD_ARREGLO)) = @cod;";

                conn.Execute(q, new { cod, desc, activo });

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = $"Tipo de Arreglo: {cod} - {desc}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_GUARDADO_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool ExisteCausa(SqlConnection conn, string codCausa)
        {
            const string q = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_CAUSAS_MOROSIDAD WHERE UPPER(RTRIM(COD_CAUSA)) = @cod;";
            int existe = conn.QueryFirstOrDefault<int>(q, new { cod = codCausa });
            return existe > 0;
        }

        private static bool ExisteArreglo(SqlConnection conn, string codArreglo)
        {
            const string q = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_TIPOS_ARREGLOS WHERE UPPER(RTRIM(COD_ARREGLO)) = @cod;";
            int existe = conn.QueryFirstOrDefault<int>(q, new { cod = codArreglo });
            return existe > 0;
        }

        private static List<COCausaMorosidadData> AplicarFiltroCausas(List<COCausaMorosidadData> lista,string? filtroIn)
        {
            string filtro = (filtroIn ?? "").Trim();
            if (string.IsNullOrWhiteSpace(filtro)) return lista;

            string qf = filtro.ToUpperInvariant();
            var filtrada = new List<COCausaMorosidadData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var it = lista[i];
                string c = (it.cod_causa ?? "").Trim().ToUpperInvariant();
                string d = (it.descripcion ?? "").Trim().ToUpperInvariant();
                string a = (it.activa ? "SI" : "NO").ToUpperInvariant();

                if (c.Contains(qf) || d.Contains(qf) || a.Contains(qf))
                    filtrada.Add(it);
            }

            return filtrada;
        }

        private static List<COArregloPagoTipoData> AplicarFiltroArreglos(List<COArregloPagoTipoData> lista, string? filtroIn)
        {
            string filtro = (filtroIn ?? "").Trim();
            if (string.IsNullOrWhiteSpace(filtro)) return lista;

            string qf = filtro.ToUpperInvariant();
            var filtrada = new List<COArregloPagoTipoData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var it = lista[i];
                string c = (it.cod_arreglo ?? "").Trim().ToUpperInvariant();
                string d = (it.descripcion ?? "").Trim().ToUpperInvariant();
                string a = it.activo ? "SI" : "NO";

                if (c.Contains(qf) || d.Contains(qf) || a.Contains(qf))
                    filtrada.Add(it);
            }

            return filtrada;
        }

        private static List<COCausaMorosidadData> AplicarSortCausas(List<COCausaMorosidadData> lista, string? sortFieldIn, int sortOrder)
        {
            string sortField = (sortFieldIn ?? "").Trim() switch
            {
                SF_CAUSA_COD => SF_CAUSA_COD,
                SF_CAUSA_DESC => SF_CAUSA_DESC,
                SF_CAUSA_ACTIVA => SF_CAUSA_ACTIVA,
                _ => SF_CAUSA_COD
            };

            bool desc = sortOrder == 0;

            lista.Sort((a, b) =>
            {
                int cmp = CompararCampoCausa(a, b, sortField);
                return desc ? -cmp : cmp;
            });

            return lista;
        }

        private static int CompararCampoCausa(COCausaMorosidadData a, COCausaMorosidadData b, string sortField)
        {
            if (sortField == SF_CAUSA_COD)
                return string.Compare(a.cod_causa ?? "", b.cod_causa ?? "", StringComparison.OrdinalIgnoreCase);

            if (sortField == SF_CAUSA_DESC)
                return string.Compare(a.descripcion ?? "", b.descripcion ?? "", StringComparison.OrdinalIgnoreCase);

            if (a.activa == b.activa) return 0;
            return a.activa ? 1 : -1;
        }

        private static List<COArregloPagoTipoData> AplicarSortArreglos(List<COArregloPagoTipoData> lista, string? sortFieldIn, int sortOrder)
        {
            string sortField = (sortFieldIn ?? "").Trim() switch
            {
                SF_ARREGLO_COD => SF_ARREGLO_COD,
                SF_ARREGLO_DESC => SF_ARREGLO_DESC,
                SF_ARREGLO_ACTIVO => SF_ARREGLO_ACTIVO,
                _ => SF_ARREGLO_COD
            };

            bool desc = sortOrder == 0;

            lista.Sort((a, b) =>
            {
                int cmp = CompararCampoArreglo(a, b, sortField);
                return desc ? -cmp : cmp;
            });

            return lista;
        }

        private static int CompararCampoArreglo(COArregloPagoTipoData a, COArregloPagoTipoData b, string sortField)
        {
            if (sortField == SF_ARREGLO_COD)
                return string.Compare(a.cod_arreglo ?? "", b.cod_arreglo ?? "", StringComparison.OrdinalIgnoreCase);

            if (sortField == SF_ARREGLO_DESC)
                return string.Compare(a.descripcion ?? "", b.descripcion ?? "", StringComparison.OrdinalIgnoreCase);

            if (a.activo == b.activo) return 0;
            return a.activo ? 1 : -1;
        }

        private static List<T> AplicarPaginacion<T>(List<T> lista, int pagina, int paginacion)
        {
            var paged = new List<T>();

            int start = pagina < 0 ? 0 : pagina;
            int take = paginacion < 0 ? 0 : paginacion;

            int end = start + take;
            if (end > lista.Count) end = lista.Count;

            for (int i = start; i < end; i++)
                paged.Add(lista[i]);

            return paged;
        }
    }
}
