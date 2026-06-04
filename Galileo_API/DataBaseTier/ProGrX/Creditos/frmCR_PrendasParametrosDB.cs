using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCreditos = 3;
        private const string MovimientoAgregar = "A";
        private const string MovimientoEliminar = "E";
        private const string MensajeOk = "Ok";
        private const string DATOSREQUERIDOS = "Datos requeridos.";
        private const string CODIGOREQUERIDO = "Código requerido.";
        private const string ELIMINADOCORRECTAMENTE = "Eliminado correctamente.";
        private const string GUARDADOCORRECTAMENTE = "Guardado correctamente.";
        private const string DESCRIPCION = "DESCRIPCION";
        private const string ACTIVA = "ACTIVA";
        private const string REGISTRO_USUARIO = "REGISTRO_USUARIO";
        private const string REGISTRO_FECHA = "REGISTRO_FECHA";
        public FrmCrPrendasParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de catálogos generales de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Obtener(int CodEmpresa, string tipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var listaResult = new CrPrendasParametrosLista<CrPrendasCatalogoData>();
            var response = CrearResponseLista(listaResult);

            try
            {
                var rows = conn.Query("exec spCrd_Prendas_Cat_List @Tipo", new { Tipo = NormalizarTexto(tipo) }).ToList();
                listaResult.lista = rows.Select(MapCatalogoData).ToList();
                listaResult.total = listaResult.lista.Count;
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasCatalogoData>>(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Exporta la lista de catálogos generales de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Export(int CodEmpresa, string tipo)
        {
            return CR_PrendasParametros_Catalogo_Lista_Obtener(CodEmpresa, tipo);
        }

        /// <summary>
        /// Obtiene el dropdown de catálogos generales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_PrendasParametros_Catalogos_Dropdown_Obtener(int CodEmpresa)
        {
            var lista = new List<DropDownListaGenericaModel>
            {
                new() { item = "COB", descripcion = "Combustible" },
                new() { item = "MAR", descripcion = "Marcas" },
                new() { item = "MOD", descripcion = "Modelos" },
                new() { item = "PRE", descripcion = "Presentación" },
                new() { item = "EXT", descripcion = "Extras" },
                new() { item = "ASE", descripcion = "Aseguradoras" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Guarda un catálogo general de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Catalogo_Guardar(int CodEmpresa, CrPrendasCatalogoGuardarRequest request, string usuario)
        {
            if (request == null)
                return DbHelper.ErrorResponse(DATOSREQUERIDOS);

            return EsNuevo(request.codigo) ? InsertarCatalogo(CodEmpresa, request, usuario) : ActualizarCatalogo(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Elimina un catálogo general de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Catalogo_Eliminar(int CodEmpresa, CrPrendasCatalogoEliminarRequest request, string usuario)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.codigo))
                return DbHelper.ErrorResponse(CODIGOREQUERIDO);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarCatalogoSp(conn, new CatalogoSpArgs { Tipo = request.tipo, Codigo = request.codigo, Descripcion = string.Empty, Activa = 0, Usuario = usuario, Mov = MovimientoEliminar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_{request.tipo} Id: {result.codigo}");
                return DbHelper.OkResponse(ELIMINADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de coberturas de pólizas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ObtenerFiltros(parametros);
            if (filtrosResult.Code != 0)
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasCoberturaData>>(filtrosResult.Description ?? string.Empty);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                return ObtenerCoberturas(conn, filtrosResult.Result ?? new FiltrosLazyLoadData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasCoberturaData>>(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Exporta la lista de coberturas de pólizas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Export(int CodEmpresa, string parametros)
        {
            return CR_PrendasParametros_Coberturas_Lista_Obtener(CodEmpresa, ForzarExport(parametros));
        }

        /// <summary>
        /// Guarda una cobertura de póliza.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Coberturas_Guardar(int CodEmpresa, CrPrendasCoberturaGuardarRequest request, string usuario)
        {
            if (request == null)
                return DbHelper.ErrorResponse(DATOSREQUERIDOS);

            return EsNuevo(request.id_cobertura) ? InsertarCobertura(CodEmpresa, request, usuario) : ActualizarCobertura(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Elimina una cobertura de póliza.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Coberturas_Eliminar(int CodEmpresa, CrPrendasCoberturaEliminarRequest request, string usuario)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.id_cobertura))
                return DbHelper.ErrorResponse(CODIGOREQUERIDO);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarCoberturaSp(conn, new CoberturaSpArgs { Codigo = request.id_cobertura, Poliza = string.Empty, CoberturaId = string.Empty, Cobertura = string.Empty, Descripcion = string.Empty, Activa = 0, Usuario = usuario, Mov = MovimientoEliminar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_Coberturas Polizas Id: {result.codigo}");
                return DbHelper.OkResponse(ELIMINADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Obtiene pólizas para búsqueda F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasPolizaF4Data>> CR_PrendasParametros_Polizas_F4_Obtener(int CodEmpresa, string? texto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtro = NormalizarTexto(texto);
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;

                const string sql = @"select rtrim(Codigo) as codigo, rtrim(Descripcion) as descripcion from Catalogo where Poliza = 'S' and (@texto = '' or Codigo like @like or Descripcion like @like) order by Codigo;";
                return DbHelper.CreateOkResponse(conn.Query<CrPrendasPolizaF4Data>(sql, new { texto = filtro, like }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPrendasPolizaF4Data>>(ex.Message ?? string.Empty, -1, new List<CrPrendasPolizaF4Data>());
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de comercializadoras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ObtenerFiltros(parametros);
            if (filtrosResult.Code != 0)
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasComercializaListaData>>(filtrosResult.Description ?? string.Empty);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                return ObtenerComercializadoras(conn, filtrosResult.Result ?? new FiltrosLazyLoadData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasComercializaListaData>>(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Exporta la lista de comercializadoras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Export(int CodEmpresa, string parametros)
        {
            return CR_PrendasParametros_Comercializa_Lista_Obtener(CodEmpresa, ForzarExport(parametros));
        }

        /// <summary>
        /// Obtiene el detalle de una comercializadora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasComercializaData> CR_PrendasParametros_Comercializa_Consulta(int CodEmpresa, int codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var data = conn.QueryFirstOrDefault<CrPrendasComercializaData>("exec spCrd_Prendas_Cat_Comercializa_Consulta @Codigo", new { Codigo = codigo }) ?? new CrPrendasComercializaData();
                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasComercializaData>(ex.Message ?? string.Empty, -1, new CrPrendasComercializaData());
            }
        }

        /// <summary>
        /// Guarda una comercializadora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Comercializa_Guardar(int CodEmpresa, CrPrendasComercializaGuardarRequest request, string usuario)
        {
            if (request == null)
                return DbHelper.ErrorResponse(DATOSREQUERIDOS);

            return request.id_comercio.GetValueOrDefault() <= 0 ? InsertarComercializa(CodEmpresa, request, usuario) : ActualizarComercializa(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Elimina una comercializadora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Comercializa_Eliminar(int CodEmpresa, CrPrendasComercializaEliminarRequest request, string usuario)
        {
            if (request == null || request.id_comercio.GetValueOrDefault() <= 0)
                return DbHelper.ErrorResponse(CODIGOREQUERIDO);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarComercializaSp(conn, CrearComercializaArgs(request, usuario, MovimientoEliminar));
                if (result.pass != 1)
                    return DbHelper.ErrorResponse(result.mensaje);

                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas> Comercializador Id: {result.codigo}");
                return DbHelper.OkResponse(ELIMINADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Obtiene comercializadoras para búsqueda F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasComercializaF4Data>> CR_PrendasParametros_Comercializa_F4_Obtener(int CodEmpresa, string? texto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtro = NormalizarTexto(texto);
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;

                const string sql = @"select Id_Comercio as id_comercio, isnull(rtrim(Cedula),'') as cedula, rtrim(Descripcion) as descripcion from crd_Prendas_Comercia where @texto = '' or cast(Id_Comercio as varchar(20)) like @like or Cedula like @like or Descripcion like @like order by Id_Comercio;";
                return DbHelper.CreateOkResponse(conn.Query<CrPrendasComercializaF4Data>(sql, new { texto = filtro, like }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPrendasComercializaF4Data>>(ex.Message ?? string.Empty, -1, new List<CrPrendasComercializaF4Data>());
            }
        }

        /// <summary>
        /// Obtiene tipos de identificación para comercializadoras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasTipoIdData>> CR_PrendasParametros_TiposId_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"select TIPO_ID as tipo_id, rtrim(Descripcion) as descripcion, isnull(LARGO_MINIMO,0) as largo_minimo from AFI_TIPOS_IDS order by Tipo_Id;";
                return DbHelper.CreateOkResponse(conn.Query<CrPrendasTipoIdData>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPrendasTipoIdData>>(ex.Message ?? string.Empty, -1, new List<CrPrendasTipoIdData>());
            }
        }

        /// <summary>
        /// Obtiene bancos autorizados para comercializadoras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasBancoData>> CR_PrendasParametros_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = conn.Query<CrPrendasBancoData>("exec spCxP_Bancos_Autorizados").ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPrendasBancoData>>(ex.Message ?? string.Empty, -1, new List<CrPrendasBancoData>());
            }
        }

        /// <summary>
        /// Obtiene cuentas bancarias de una identificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasCuentaData>> CR_PrendasParametros_Cuentas_Lista_Obtener(int CodEmpresa, string identificacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"select rtrim(C.CUENTA_INTERNA) as cuenta, rtrim(B.Descripcion) as banco, case when C.tipo = 'A' then 'Ahorros' else 'Corriente' end as tipo, rtrim(C.cod_Divisa) as divisa, cast(isnull(C.CUENTA_INTERBANCA,0) as bit) as interbanca, isnull(rtrim(C.DESTINO),'') as destino, cast(isnull(C.ACTIVA,0) as bit) as activa, C.REGISTRO_FECHA as fecha, isnull(rtrim(C.REGISTRO_USUARIO),'') as usuario from SYS_CUENTAS_BANCARIAS C inner join TES_BANCOS_GRUPOS B on C.cod_banco = B.cod_grupo where C.Identificacion = @identificacion;";
                return DbHelper.CreateOkResponse(conn.Query<CrPrendasCuentaData>(sql, new { identificacion = NormalizarTexto(identificacion) }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPrendasCuentaData>>(ex.Message ?? string.Empty, -1, new List<CrPrendasCuentaData>());
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de unidades de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ObtenerFiltros(parametros);
            if (filtrosResult.Code != 0)
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasUnidadData>>(filtrosResult.Description ?? string.Empty);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                return ObtenerUnidades(conn, filtrosResult.Result ?? new FiltrosLazyLoadData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasParametrosLista<CrPrendasUnidadData>>(ex.Message ?? string.Empty);
            }
        }

        /// <summary>
        /// Exporta la lista de unidades de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Export(int CodEmpresa, string parametros)
        {
            return CR_PrendasParametros_Unidades_Lista_Obtener(CodEmpresa, ForzarExport(parametros));
        }

        /// <summary>
        /// Guarda una unidad de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Unidades_Guardar(int CodEmpresa, CrPrendasUnidadGuardarRequest request, string usuario)
        {
            if (request == null)
                return DbHelper.ErrorResponse(DATOSREQUERIDOS);

            if (string.IsNullOrWhiteSpace(request.codigo))
                return DbHelper.ErrorResponse("Debe de indicar un código para la unidad.");

            return ExisteUnidad(CodEmpresa, request.codigo) ? ActualizarUnidad(CodEmpresa, request, usuario) : InsertarUnidad(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Elimina una unidad de prendas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_PrendasParametros_Unidades_Eliminar(int CodEmpresa, CrPrendasUnidadEliminarRequest request, string usuario)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.codigo))
                return DbHelper.ErrorResponse(CODIGOREQUERIDO);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarUnidadSp(conn, new UnidadSpArgs { Codigo = request.codigo, Descripcion = string.Empty, Peso = 0, Capacidad = 0, Cilindraje = 0, Activa = 0, Usuario = usuario, Mov = MovimientoEliminar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_Unidades Id: {result.codigo}");
                return DbHelper.OkResponse(ELIMINADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        private ErrorDto InsertarCatalogo(int CodEmpresa, CrPrendasCatalogoGuardarRequest request, string usuario)
        {
            return GuardarCatalogo(CodEmpresa, request, usuario);
        }

        private ErrorDto ActualizarCatalogo(int CodEmpresa, CrPrendasCatalogoGuardarRequest request, string usuario)
        {
            return GuardarCatalogo(CodEmpresa, request, usuario);
        }

        private ErrorDto GuardarCatalogo(int CodEmpresa, CrPrendasCatalogoGuardarRequest request, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarCatalogoSp(conn, new CatalogoSpArgs { Tipo = request.tipo, Codigo = request.codigo, Descripcion = request.descripcion, Activa = BoolToSmallInt(request.activa.GetValueOrDefault()), Usuario = usuario, Mov = MovimientoAgregar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_{request.tipo} Id: {result.codigo}");
                return DbHelper.OkResponse(GUARDADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        private static CrPrendasParametroSpResult EjecutarCatalogoSp(SqlConnection conn, CatalogoSpArgs data)
        {
            const string sql = @"exec spCrd_Prendas_Cat_Parametros_Add @Tipo, @Codigo, @Descripcion, @Activa, @Usuario, @Mov;";
            data.Tipo = NormalizarTexto(data.Tipo);
            data.Codigo = NormalizarCodigo(data.Codigo);
            data.Descripcion = NormalizarTexto(data.Descripcion);
            data.Usuario = NormalizarTexto(data.Usuario);
            return conn.QueryFirstOrDefault<CrPrendasParametroSpResult>(sql, data) ?? new CrPrendasParametroSpResult();
        }

        private ErrorDto InsertarCobertura(int CodEmpresa, CrPrendasCoberturaGuardarRequest request, string usuario)
        {
            return GuardarCobertura(CodEmpresa, request, usuario);
        }

        private ErrorDto ActualizarCobertura(int CodEmpresa, CrPrendasCoberturaGuardarRequest request, string usuario)
        {
            return GuardarCobertura(CodEmpresa, request, usuario);
        }

        private ErrorDto GuardarCobertura(int CodEmpresa, CrPrendasCoberturaGuardarRequest request, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarCoberturaSp(conn, new CoberturaSpArgs { Codigo = request.id_cobertura, Poliza = request.cod_poliza, CoberturaId = request.cod_cobertura, Cobertura = request.cobertura, Descripcion = request.descripcion, Activa = BoolToSmallInt(request.activa.GetValueOrDefault()), Usuario = usuario, Mov = MovimientoAgregar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_Coberturas Polizas Id: {result.codigo}");
                return DbHelper.OkResponse(GUARDADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        private static CrPrendasParametroSpResult EjecutarCoberturaSp(SqlConnection conn, CoberturaSpArgs data)
        {
            const string sql = @"exec spCrd_Prendas_Cat_Coberturas_Add @Codigo, @Poliza, @CoberturaId, @Cobertura, @Descripcion, @Activa, @Usuario, @Mov;";
            data.Codigo = NormalizarCodigo(data.Codigo);
            data.Poliza = NormalizarTexto(data.Poliza);
            data.CoberturaId = NormalizarTexto(data.CoberturaId);
            data.Cobertura = NormalizarTexto(data.Cobertura);
            data.Descripcion = NormalizarTexto(data.Descripcion);
            data.Usuario = NormalizarTexto(data.Usuario);
            return conn.QueryFirstOrDefault<CrPrendasParametroSpResult>(sql, data) ?? new CrPrendasParametroSpResult();
        }

        private static ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> ObtenerCoberturas(SqlConnection conn, FiltrosLazyLoadData filtros)
        {
            var (texto, like, offset, fetch, usarPaginacion) = NormalizarFiltros(filtros);
            var orden = ObtenerOrdenCoberturas(filtros);

            const string sqlCount = @"select count(1) from CRD_PRENDAS_POLIZAS_COBERTURAS where @texto is null or cast(ID_COBERTURA as varchar(20)) like @like or COD_POLIZA like @like or COD_COBERTURA like @like or COBERTURA like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like;";
            var sqlLista = $@"select cast(ID_COBERTURA as varchar(20)) as id_cobertura, isnull(rtrim(COD_POLIZA),'') as cod_poliza, isnull(rtrim(COD_COBERTURA),'') as cod_cobertura, isnull(rtrim(COBERTURA),'') as cobertura, isnull(rtrim(DESCRIPCION),'') as descripcion, cast(isnull(ACTIVA,0) as bit) as activa, isnull(rtrim(REGISTRO_USUARIO),'') as usuario, REGISTRO_FECHA as fecha from CRD_PRENDAS_POLIZAS_COBERTURAS where @texto is null or cast(ID_COBERTURA as varchar(20)) like @like or COD_POLIZA like @like or COD_COBERTURA like @like or COBERTURA like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like order by {orden}{CrearPaginacion(usarPaginacion)};";
            return EjecutarListaPaginada<CrPrendasCoberturaData>(conn, sqlCount, sqlLista, new { texto, like, offset, fetch });
        }

        private ErrorDto InsertarComercializa(int CodEmpresa, CrPrendasComercializaGuardarRequest request, string usuario)
        {
            return GuardarComercializa(CodEmpresa, request, usuario);
        }

        private ErrorDto ActualizarComercializa(int CodEmpresa, CrPrendasComercializaGuardarRequest request, string usuario)
        {
            return GuardarComercializa(CodEmpresa, request, usuario);
        }

        private ErrorDto GuardarComercializa(int CodEmpresa, CrPrendasComercializaGuardarRequest request, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarComercializaSp(conn, CrearComercializaArgs(request, usuario, MovimientoAgregar));
                if (result.pass != 1)
                    return DbHelper.ErrorResponse(result.mensaje);

                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas> Comercializador Id: {result.codigo}");
                return DbHelper.OkResponse(GUARDADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        private static CrPrendasParametroSpResult EjecutarComercializaSp(SqlConnection conn, ComercializaSpArgs data)
        {
            const string sql = @"exec spCrd_Prendas_Cat_Comercializa_Add @Codigo, @TipoId, @Identificacion, @Nombre, @Activa, @BancoId, @Correo, @Usuario, @Mov;";
            return conn.QueryFirstOrDefault<CrPrendasParametroSpResult>(sql, data) ?? new CrPrendasParametroSpResult();
        }

        private static ComercializaSpArgs CrearComercializaArgs(CrPrendasComercializaGuardarRequest request, string usuario, string movimiento)
        {
            return new ComercializaSpArgs { Codigo = request.id_comercio.GetValueOrDefault(), TipoId = request.tipo_id.GetValueOrDefault(), Identificacion = NormalizarTexto(request.cedula), Nombre = NormalizarTexto(request.descripcion), Activa = BoolToSmallInt(request.activa.GetValueOrDefault()), BancoId = request.id_banco.GetValueOrDefault(), Correo = NormalizarTexto(request.correo), Usuario = NormalizarTexto(usuario), Mov = movimiento };
        }

        private static ComercializaSpArgs CrearComercializaArgs(CrPrendasComercializaEliminarRequest request, string usuario, string movimiento)
        {
            return new ComercializaSpArgs { Codigo = request.id_comercio.GetValueOrDefault(), TipoId = request.tipo_id.GetValueOrDefault(), Identificacion = NormalizarTexto(request.cedula), Nombre = NormalizarTexto(request.descripcion), Activa = BoolToSmallInt(request.activa.GetValueOrDefault()), BancoId = request.id_banco.GetValueOrDefault(), Correo = NormalizarTexto(request.correo), Usuario = NormalizarTexto(usuario), Mov = movimiento };
        }

        private static ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> ObtenerComercializadoras(SqlConnection conn, FiltrosLazyLoadData filtros)
        {
            var (texto, like, offset, fetch, usarPaginacion) = NormalizarFiltros(filtros);
            var orden = ObtenerOrdenComercializa(filtros);

            const string sqlCount = @"select count(1) from CRD_PRENDAS_COMERCIA where @texto is null or cast(ID_COMERCIO as varchar(20)) like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like;";
            var sqlLista = $@"select ID_COMERCIO as id_comercio, isnull(rtrim(DESCRIPCION),'') as descripcion, cast(isnull(ACTIVA,0) as bit) as activa, isnull(rtrim(REGISTRO_USUARIO),'') as usuario, REGISTRO_FECHA as fecha from CRD_PRENDAS_COMERCIA where @texto is null or cast(ID_COMERCIO as varchar(20)) like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like order by {orden}{CrearPaginacion(usarPaginacion)};";
            return EjecutarListaPaginada<CrPrendasComercializaListaData>(conn, sqlCount, sqlLista, new { texto, like, offset, fetch });
        }

        private ErrorDto InsertarUnidad(int CodEmpresa, CrPrendasUnidadGuardarRequest request, string usuario)
        {
            return GuardarUnidad(CodEmpresa, request, usuario);
        }

        private ErrorDto ActualizarUnidad(int CodEmpresa, CrPrendasUnidadGuardarRequest request, string usuario)
        {
            return GuardarUnidad(CodEmpresa, request, usuario);
        }

        private ErrorDto GuardarUnidad(int CodEmpresa, CrPrendasUnidadGuardarRequest request, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarUnidadSp(conn, new UnidadSpArgs { Codigo = request.codigo, Descripcion = request.descripcion, Peso = BoolToSmallInt(request.peso_apl.GetValueOrDefault()), Capacidad = BoolToSmallInt(request.capacidad_apl.GetValueOrDefault()), Cilindraje = BoolToSmallInt(request.cilindraje_apl.GetValueOrDefault()), Activa = BoolToSmallInt(request.activa.GetValueOrDefault()), Usuario = usuario, Mov = MovimientoAgregar });
                RegistrarBitacora(CodEmpresa, usuario, result.movimiento, $"Prendas Cat_Unidades Id: {result.codigo}");
                return DbHelper.OkResponse(GUARDADOCORRECTAMENTE);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? string.Empty);
            }
        }

        private bool ExisteUnidad(int CodEmpresa, string codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            const string sql = @"select count(1) from CRD_PRENDAS_UDS where ID_UNIDAD = @codigo;";
            return conn.QueryFirstOrDefault<int>(sql, new { codigo = NormalizarTexto(codigo) }) > 0;
        }

        private static CrPrendasParametroSpResult EjecutarUnidadSp(SqlConnection conn, UnidadSpArgs data)
        {
            const string sql = @"exec spCrd_Prendas_Cat_Unidades_Add @Codigo, @Descripcion, @Peso, @Capacidad, @Cilindraje, @Activa, @Usuario, @Mov;";
            data.Codigo = NormalizarTexto(data.Codigo);
            data.Descripcion = NormalizarTexto(data.Descripcion);
            data.Usuario = NormalizarTexto(data.Usuario);
            return conn.QueryFirstOrDefault<CrPrendasParametroSpResult>(sql, data) ?? new CrPrendasParametroSpResult();
        }

        private static ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> ObtenerUnidades(SqlConnection conn, FiltrosLazyLoadData filtros)
        {
            var (texto, like, offset, fetch, usarPaginacion) = NormalizarFiltros(filtros);
            var orden = ObtenerOrdenUnidades(filtros);

            const string sqlCount = @"select count(1) from CRD_PRENDAS_UDS where @texto is null or ID_UNIDAD like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like;";
            var sqlLista = $@"select isnull(rtrim(ID_UNIDAD),'') as codigo, isnull(rtrim(DESCRIPCION),'') as descripcion, cast(isnull(PESO_APL,0) as bit) as peso_apl, cast(isnull(CAPACIDAD_APL,0) as bit) as capacidad_apl, cast(isnull(CILINDRAJE_APL,0) as bit) as cilindraje_apl, cast(isnull(ACTIVA,0) as bit) as activa, isnull(rtrim(REGISTRO_USUARIO),'') as usuario, REGISTRO_FECHA as fecha from CRD_PRENDAS_UDS where @texto is null or ID_UNIDAD like @like or DESCRIPCION like @like or REGISTRO_USUARIO like @like order by {orden}{CrearPaginacion(usarPaginacion)};";
            return EjecutarListaPaginada<CrPrendasUnidadData>(conn, sqlCount, sqlLista, new { texto, like, offset, fetch });
        }

        private static ErrorDto<CrPrendasParametrosLista<T>> CrearResponseLista<T>(CrPrendasParametrosLista<T> result)
        {
            return new ErrorDto<CrPrendasParametrosLista<T>> { Code = 0, Description = MensajeOk, Result = result };
        }

        private static ErrorDto<FiltrosLazyLoadData> ObtenerFiltros(string parametros)
        {
            try
            {
                return DbHelper.CreateOkResponse(JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData());
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(ex.Message ?? string.Empty, -1, new FiltrosLazyLoadData());
            }
        }

        private static string ForzarExport(string parametros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return JsonConvert.SerializeObject(filtros);
        }

        private static (string? texto, string? like, int offset, int fetch, bool usarPaginacion) NormalizarFiltros(FiltrosLazyLoadData filtros)
        {
            var textoNormalizado = NormalizarTexto(filtros.filtro);
            var texto = textoNormalizado.Length > 0 ? textoNormalizado : null;
            var like = textoNormalizado.Length > 0 ? $"%{textoNormalizado}%" : null;
            var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
            var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;
            return (texto, like, offset, fetch, fetch > 0);
        }

        private static ErrorDto<CrPrendasParametrosLista<T>> EjecutarListaPaginada<T>(SqlConnection conn, string sqlCount, string sqlLista, object parametros)
        {
            var listaResult = new CrPrendasParametrosLista<T>();
            var response = CrearResponseLista(listaResult);
            listaResult.total = conn.QuerySingle<int>(sqlCount, parametros);
            listaResult.lista = conn.Query<T>(sqlLista, parametros).ToList();
            return response;
        }

        private static string CrearPaginacion(bool usarPaginacion)
        {
            return usarPaginacion ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY " : string.Empty;
        }

        private static string ObtenerOrdenCoberturas(FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var direction = filtros.sortOrder == -1 ? "desc" : "asc";
            var column = sortField switch { "id_cobertura" => "ID_COBERTURA", "cod_poliza" => "COD_POLIZA", "cod_cobertura" => "COD_COBERTURA", "cobertura" => "COBERTURA", "descripcion" => DESCRIPCION, "activa" => ACTIVA, "usuario" => REGISTRO_USUARIO, "fecha" => REGISTRO_FECHA, _ => "ID_COBERTURA" };
            return $"{column} {direction}";
        }

        private static string ObtenerOrdenComercializa(FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var direction = filtros.sortOrder == -1 ? "desc" : "asc";
            var column = sortField switch { "id_comercio" => "ID_COMERCIO", "descripcion" => DESCRIPCION, "activa" => ACTIVA, "usuario" => REGISTRO_USUARIO, "fecha" => REGISTRO_FECHA, _ => "ACTIVA desc, DESCRIPCION" };
            return column.Contains(',', StringComparison.Ordinal) ? column : $"{column} {direction}";
        }

        private static string ObtenerOrdenUnidades(FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var direction = filtros.sortOrder == -1 ? "desc" : "asc";
            var column = sortField switch { "codigo" => "ID_UNIDAD", "descripcion" => DESCRIPCION, "peso_apl" => "PESO_APL", "capacidad_apl" => "CAPACIDAD_APL", "cilindraje_apl" => "CILINDRAJE_APL", "activa" => ACTIVA, "usuario" => REGISTRO_USUARIO, "fecha" => REGISTRO_FECHA, _ => "ID_UNIDAD" };
            return $"{column} {direction}";
        }

        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto { EmpresaId = CodEmpresa, Usuario = NormalizarTexto(usuario), Movimiento = movimiento, DetalleMovimiento = detalle, Modulo = ModuloCreditos });
        }

        private static CrPrendasCatalogoData MapCatalogoData(dynamic row)
        {
            var dic = (IDictionary<string, object>)row;
            return new CrPrendasCatalogoData { codigo = Convert.ToString(dic.Values.FirstOrDefault()) ?? string.Empty, descripcion = Convert.ToString(GetValue(dic, DESCRIPCION)) ?? string.Empty, activa = ToBool(GetValue(dic, "ACTIVO") ?? GetValue(dic, ACTIVA)), usuario = Convert.ToString(GetValue(dic, REGISTRO_USUARIO)) ?? string.Empty, fecha = ToNullableDate(GetValue(dic, REGISTRO_FECHA)) };
        }

        private static object? GetValue(IDictionary<string, object> dic, string key)
        {
            return dic.TryGetValue(key, out var value) ? value : null;
        }

        private static bool EsNuevo(string codigo)
        {
            return string.IsNullOrWhiteSpace(codigo) || codigo.Trim() == "0";
        }

        private static string NormalizarCodigo(string codigo)
        {
            var valor = NormalizarTexto(codigo);
            return valor.Length == 0 ? "0" : valor;
        }

        private static string NormalizarTexto(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static int BoolToSmallInt(bool value)
        {
            return value ? 1 : 0;
        }

        private static bool ToBool(object? value)
        {
            return value != null && Convert.ToInt32(value) != 0;
        }

        private static DateTime? ToNullableDate(object? value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ToDateTime(value);
        }

        private sealed class CatalogoSpArgs
        {
            public string Tipo { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int Activa { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string Mov { get; set; } = string.Empty;
        }

        private sealed class CoberturaSpArgs
        {
            public string Codigo { get; set; } = string.Empty;
            public string Poliza { get; set; } = string.Empty;
            public string CoberturaId { get; set; } = string.Empty;
            public string Cobertura { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int Activa { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string Mov { get; set; } = string.Empty;
        }

        private sealed class ComercializaSpArgs
        {
            public int Codigo { get; set; }
            public int TipoId { get; set; }
            public string Identificacion { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public int Activa { get; set; }
            public int BancoId { get; set; }
            public string Correo { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Mov { get; set; } = string.Empty;
        }

        private sealed class UnidadSpArgs
        {
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int Peso { get; set; }
            public int Capacidad { get; set; }
            public int Cilindraje { get; set; }
            public int Activa { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string Mov { get; set; } = string.Empty;
        }
    }
}