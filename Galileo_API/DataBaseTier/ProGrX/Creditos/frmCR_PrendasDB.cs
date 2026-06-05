using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using System.Globalization;
using System.Linq;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasDb
    {
        private const string ColPrendaId = "Prenda_Id";
        private const string ColRegistroFecha = "Registro_fecha";
        private const string ColRegistroUsuario = "Registro_Usuario";
        private const string ColActualizaFecha = "ACTUALIZA_FECHA";
        private const string ColActualizaUsuario = "ACTUALIZA_USUARIO";
        private const string FormatoFechaSql = "yyyy-MM-dd";

        private readonly PortalDB _portalDb;

        public FrmCrPrendasDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCrPrendasDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        private ErrorDto<List<T>> EjecutarListaMapeada<T>(
            int codEmpresa,
            string sql,
            object parametros,
            Func<dynamic, T> mapear)
        {
            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query(sql, parametros)
                    .Select(mapear)
                    .ToList());

            return new ErrorDto<List<T>>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result = resp.Result ?? new List<T>()
            };
        }

        private ErrorDto<T> EjecutarUnicoMapeado<T>(
            int codEmpresa,
            string sql,
            object parametros,
            Func<dynamic, T> mapear)
            where T : new()
        {
            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query(sql, parametros)
                    .Select(mapear)
                    .FirstOrDefault());

            return new ErrorDto<T>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result = resp.Result ?? new T()
            };
        }

        private ErrorDto<string> EjecutarRespuestaSp(
            int codEmpresa,
            string sql,
            object parametros,
            string mensajeOk)
        {
            var resp = EjecutarPrimerResultado(codEmpresa, sql, parametros);

            return RespuestaSp(resp, mensajeOk);
        }

        private dynamic EjecutarPrimerResultado(int codEmpresa, string sql, object parametros)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query(sql, parametros).FirstOrDefault());
        }

        public ErrorDto<List<CrPrendaListaData>> CrPrendas_Obtener(
            int codEmpresa,
            long operacion,
            string expediente)
        {
            const string sql = "exec spCrd_Prendas_List @Operacion, @Expediente;";

            return EjecutarListaMapeada(
                codEmpresa,
                sql,
                new { Operacion = operacion, Expediente = expediente ?? string.Empty },
                MapearPrendaLista);
        }

        public ErrorDto<CrPrendaDetalleData> CrPrendas_ObtenerDetalle(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Garantia_Load @PrendaId;";

            return EjecutarUnicoMapeado(codEmpresa, sql, new { PrendaId = prendaId }, MapearPrendaDetalle);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_TiposActivos(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(tipo_prenda) as idx,
                    rtrim(descripcion) as itmx
                from crd_prendas_tipos
                where Activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_CatalogoLista(int codEmpresa, string tipoCatalogo)
        {
            const string sql = "exec spCrd_Prendas_Cat_List_Cbo @TipoCatalogo;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { TipoCatalogo = (tipoCatalogo ?? string.Empty).Trim().ToUpperInvariant() });
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_UnidadesLista(int codEmpresa, string aplicacion)
        {
            var campo = (aplicacion ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "PESO" => "Peso_Apl",
                "CAPACIDAD" => "Capacidad_Apl",
                "CILINDRAJE" => "Cilindraje_Apl",
                _ => "Peso_Apl"
            };

            var sql = $@"
                select rtrim(ID_Unidad) as idx, rtrim(descripcion) as itmx
                from CRD_PRENDAS_uds
                where {campo} = 1 and Activa = 1
                order by Descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_ParentescosLista(int codEmpresa)
        {
            const string sql = @"
                select rtrim(cod_Parentesco) as idx, rtrim(Descripcion) as itmx
                from sys_Parentescos
                where activo = 1
                order by Descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_TiposIdentificacionLista(int codEmpresa)
        {
            const string sql = @"
                select
                    convert(varchar(10), TIPO_ID) as idx,
                    rtrim(Descripcion) as itmx,
                    isnull(LARGO_MINIMO, 0) as largo_minimo
                from AFI_TIPOS_IDS
                where TIPO_PERSONERIA = 'F'
                order by Tipo_Id;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<CrPrendaAnotacionData>> CrPrendas_AnotacionesLista(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Anotaciones_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearAnotacion);
        }

        public ErrorDto<List<CrPrendaPolizaCoberturaData>> CrPrendas_PolizasList(int codEmpresa, string tipoPrenda, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_List @TipoPrenda, @PrendaId;";

            return EjecutarListaMapeada(
                codEmpresa,
                sql,
                new { TipoPrenda = tipoPrenda ?? string.Empty, PrendaId = prendaId },
                MapearPolizaCobertura);
        }

        public ErrorDto<List<CrPrendaHistoricoAvaluoData>> CrPrendas_AvaluosLista(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Avaluos_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearHistoricoAvaluo);
        }

        public ErrorDto<string> CrPrendas_AvaluoGuardar(int codEmpresa, CrPrendaAvaluoGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Avaluo
                    @PrendaId,
                    @Inspector,
                    @ValorTotal,
                    @PorcCobertura,
                    @Cobertura,
                    @Notas,
                    @FechaInspeccion,
                    @ValorFiscal,
                    @ValorFinal,
                    @MontoExtras,
                    @PolizaFactor,
                    @PolizaFormaliza,
                    @PolizaRstPlan,
                    @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Inspector = request.inspector ?? string.Empty,
                    ValorTotal = request.valor_total,
                    Cobertura = request.cobertura,
                    PorcCobertura = request.porc_cobertura,
                    Notas = request.notas ?? string.Empty,
                    FechaInspeccion = ConvertirFechaSql(request.fecha_inspeccion),
                    ValorFiscal = request.valor_fiscal,
                    ValorFinal = request.valor_total,
                    MontoExtras = request.monto_extras,
                    PolizaFactor = request.poliza_factor,
                    PolizaFormaliza = request.poliza_formaliza,
                    PolizaRstPlan = request.poliza_rst_plan,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "AvalÃºo registrado satisfactoriamente!");
        }

        public ErrorDto<string> CrPrendas_NotariadoGuardar(int codEmpresa, CrPrendaNotariadoGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Notariado
                    @PrendaId,
                    @Notario,
                    @Tomo,
                    @Folio,
                    @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Notario = request.notario ?? string.Empty,
                    Tomo = request.tomo ?? string.Empty,
                    Folio = request.folio ?? string.Empty,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "InformaciÃ³n de notariado actualizada satisfactoriamente!");
        }

        public ErrorDto<string> CrPrendas_NotaGuardar(int codEmpresa, CrPrendaNotaGuardarRequest request)
        {
            const string sql = "exec spCrd_Prendas_Anotaciones_Add @PrendaId, @Nota, @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Nota = request.nota ?? string.Empty,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "Registro de Notas de Prenda realizado Satisfactoriamente!");
        }

        public ErrorDto<string> CrPrendas_PolizaCoberturaGuardar(
            int codEmpresa,
            CrPrendaPolizaCoberturaGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Prendas_Polizas_Add
                    @PrendaId,
                    @CoberturaId,
                    @Usuario,
                    @Asignado;";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new
            {
                PrendaId = request.prenda_id,
                CoberturaId = request.id_prenda_cobertura,
                Usuario = UsuarioNormalizado(request.usuario),
                Asignado = request.asignado
            });

            return new ErrorDto<string>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result = resp.Code == -1 ? string.Empty : "Cobertura de pÃ³liza actualizada correctamente."
            };
        }

        public ErrorDto<List<CrPrendaHistoricoPolizaData>> CrPrendas_PolizasExternasLista(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_Externas_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearHistoricoPoliza);
        }

        public ErrorDto<CrPrendaDetalleData> CrPrendas_PolizaExternaLoad(int codEmpresa, long prendaId, int polizaExtId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_Externas_Load @PrendaId, @PolizaExtId;";

            return EjecutarUnicoMapeado(
                codEmpresa,
                sql,
                new { PrendaId = prendaId, PolizaExtId = polizaExtId },
                MapearPolizaExternaDetalle);
        }

        public ErrorDto<long> CrPrendas_Guardar(int codEmpresa, CrPrendaGuardarCompletaRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Registro
                    @PrendaId,
                    @Operacion,
                    @Expediente,
                    @Identificacion,
                    @TipoPrenda,
                    @IdPrincipal,
                    @IdProvisional,
                    @Descripcion,
                    @Observaciones,
                    @Marca,
                    @Modelo,
                    @Serie,
                    @Color,
                    @Anio,
                    @Peso,
                    @Capacidad,
                    @Cilindraje,
                    @Puertas,
                    @Chasis,
                    @Vin,
                    @IdMarca,
                    @IdModelo,
                    @IdPresentacion,
                    @IdCombustible,
                    @IdComercio,
                    @PesoUd,
                    @CapacidadUd,
                    @CilindrajeUd,
                    @Avaluo,
                    @PorcCobertura,
                    @Cobertura,
                    @AvaluoNotas,
                    @FechaInspeccion,
                    @ValorFiscal,
                    @ValorTotal,
                    @MontoExtras,
                    @PolizaFactor,
                    @PolizaFormaliza,
                    @PolizaRstPlan,
                    @Usuario,
                    @TitularTercero,
                    @TitularNombre,
                    @Inspector;";

            var parametros = CrearParametrosGuardar(request);
            var resp = EjecutarPrimerResultado(codEmpresa, sql, parametros);

            return CrearRespuestaGuardar(resp);
        }

        private static object CrearParametrosGuardar(CrPrendaGuardarCompletaRequest request)
        {
            var vehiculo = CrearParametrosVehiculo(request);

            return new
            {
                PrendaId = request.prenda_id,
                Operacion = request.operacion,
                Expediente = request.expediente ?? string.Empty,
                Identificacion = request.identificacion ?? string.Empty,
                TipoPrenda = request.tipo_prenda ?? string.Empty,
                IdPrincipal = request.id_principal ?? string.Empty,
                IdProvisional = request.id_provisional ?? string.Empty,
                Descripcion = request.descripcion ?? string.Empty,
                Observaciones = request.observaciones ?? string.Empty,
                Marca = request.marca ?? string.Empty,
                Modelo = request.modelo ?? string.Empty,
                Serie = request.serie ?? string.Empty,
                Color = request.color ?? string.Empty,
                vehiculo.Anio,
                vehiculo.Peso,
                vehiculo.Capacidad,
                vehiculo.Cilindraje,
                vehiculo.Puertas,
                vehiculo.Chasis,
                vehiculo.Vin,
                vehiculo.IdMarca,
                vehiculo.IdModelo,
                vehiculo.IdPresentacion,
                vehiculo.IdCombustible,
                vehiculo.IdComercio,
                PesoUd = request.peso_ud ?? string.Empty,
                CapacidadUd = request.capacidad_ud ?? string.Empty,
                CilindrajeUd = request.cilindraje_ud ?? string.Empty,
                Avaluo = request.avaluo,
                PorcCobertura = request.porc_cobertura,
                Cobertura = request.cobertura,
                AvaluoNotas = request.avaluo_observacion ?? string.Empty,
                FechaInspeccion = ConvertirFechaSql(request.avaluo_inspeccion),
                ValorFiscal = request.valor_fiscal,
                ValorTotal = request.avaluo,
                MontoExtras = request.monto_extras,
                PolizaFactor = request.poliza_factor,
                PolizaFormaliza = request.poliza_mnt_formalizacion,
                PolizaRstPlan = request.poliza_rst_plan,
                Usuario = UsuarioNormalizado(request.usuario),
                TitularTercero = request.titular_tercero,
                TitularNombre = request.titular_nombre ?? string.Empty,
                Inspector = request.avaluo_inspector ?? string.Empty
            };
        }

        private sealed class ParametrosVehiculoGuardar
        {
            public int Anio { get; set; } = 0;
            public decimal Peso { get; set; } = 0;
            public decimal Capacidad { get; set; } = 0;
            public decimal Cilindraje { get; set; } = 0;
            public decimal Puertas { get; set; } = 0;
            public string Chasis { get; set; } = string.Empty;
            public string Vin { get; set; } = string.Empty;
            public int IdMarca { get; set; } = 0;
            public int IdModelo { get; set; } = 0;
            public int IdPresentacion { get; set; } = 0;
            public int IdCombustible { get; set; } = 0;
            public int IdComercio { get; set; } = 0;
        }

        private static ParametrosVehiculoGuardar CrearParametrosVehiculo(CrPrendaGuardarCompletaRequest request)
        {
            if (!request.es_vehicular)
            {
                return new ParametrosVehiculoGuardar
                {
                    Anio = ObtenerEnteroSeguro(request.anio)
                };
            }

            return new ParametrosVehiculoGuardar
            {
                Anio = ObtenerEnteroSeguro(request.anio),
                Peso = request.peso,
                Capacidad = request.capacidad,
                Cilindraje = request.cilindraje,
                Puertas = request.puertas_numero,
                Chasis = request.chasis_numero ?? string.Empty,
                Vin = request.vin_motor ?? string.Empty,
                IdMarca = request.id_marca,
                IdModelo = request.id_modelo,
                IdPresentacion = request.id_presentacion,
                IdCombustible = request.id_combustible,
                IdComercio = request.id_comercio
            };
        }

        private static ErrorDto<long> CrearRespuestaGuardar(dynamic resp)
        {
            if (resp.Code == -1)
            {
                return new ErrorDto<long>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = 0
                };
            }

            if (resp.Result == null)
            {
                return new ErrorDto<long>
                {
                    Code = -1,
                    Description = "No se obtuvo respuesta al guardar la garantia prendaria.",
                    Result = 0
                };
            }

            return CrearRespuestaGuardarExitosa(resp.Result);
        }

        private static ErrorDto<long> CrearRespuestaGuardarExitosa(dynamic result)
        {
            var valores = CrearDiccionario(result);
            var pass = ObtenerInt(valores, "Pass");
            var mensaje = ObtenerString(valores, "Mensaje");
            var movimiento = ObtenerString(valores, "Movimiento");
            var prendaId = ObtenerLong(valores, "PrendaId");

            return new ErrorDto<long>
            {
                Code = pass == 1 ? 0 : -1,
                Description = pass == 1
                    ? $"Se ha {movimiento} satisfactoriamente, la prenda Id: {prendaId}"
                    : mensaje,
                Result = prendaId
            };
        }

        private static object CrearParametrosPolizaExterna(CrPrendaPolizaExternaGuardarRequest request)
        {
            return new
            {
                PrendaId = request.prenda_id,
                AseguradoraId = request.id_aseguradora,
                NumeroPoliza = request.pe_numero ?? string.Empty,
                Prima = request.pe_prima,
                Frecuencia = request.pe_frecuencia ?? string.Empty,
                Inicio = ConvertirFechaSql(request.pe_inicio),
                Corte = ConvertirFechaSql(request.pe_vence),
                Activa = request.pe_activa,
                PolizaIndica = request.pe_indica,
                Cobertura = request.pe_cobertura ?? string.Empty,
                Notas = request.pe_notas ?? string.Empty,
                Usuario = UsuarioNormalizado(request.usuario),
                TipoId = ObtenerEnteroSeguro(request.a_tipo_id),
                Cedula = request.a_cedula ?? string.Empty,
                Apellido1 = request.a_apellido_1 ?? string.Empty,
                Apellido2 = request.a_apellido_2 ?? string.Empty,
                Nombre = request.a_nombre ?? string.Empty,
                Nacimiento = ConvertirFechaSql(request.a_nacimiento),
                Sexo = ObtenerInicial(request.a_sexo),
                Email = request.a_email ?? string.Empty,
                Telefono = request.a_tel_movil ?? string.Empty,
                Parentesco = request.a_cod_parentesco ?? string.Empty,
                PolizaExtId = request.pe_id
            };
        }

        public ErrorDto<string> CrPrendas_PolizaExternaGuardar(
            int codEmpresa,
            CrPrendaPolizaExternaGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Poliza_Externa_Registra
                    @PrendaId,
                    @AseguradoraId,
                    @NumeroPoliza,
                    @Prima,
                    @Frecuencia,
                    @Inicio,
                    @Corte,
                    @Activa,
                    @PolizaIndica,
                    @Cobertura,
                    @Notas,
                    @Usuario,
                    @TipoId,
                    @Cedula,
                    @Apellido1,
                    @Apellido2,
                    @Nombre,
                    @Nacimiento,
                    @Sexo,
                    @Email,
                    @Telefono,
                    @Parentesco,
                    @PolizaExtId;";

            var resp = EjecutarPrimerResultado(
                codEmpresa,
                sql,
                CrearParametrosPolizaExterna(request));

            return CrearRespuestaMovimiento(
                resp,
                "No se obtuvo respuesta al registrar la pÃ³liza externa.",
                new Func<string, string>(movimiento => $"PÃ³liza Externa {movimiento} satisfactoriamente!"));
        }

        public ErrorDto CrPrendas_Eliminar(int codEmpresa, CrPrendasEliminarRequest request)
        {
            const string sql = "exec spCrd_Operacion_Prenda_Elimina @PrendaId, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Usuario = UsuarioNormalizado(request.usuario)
                });
        }

        private static CrPrendaListaData MapearPrendaLista(dynamic row)
        {
            var valores = CrearDiccionario(row);
            var prenda = new CrPrendaListaData();

            CompletarPrendaBase(prenda, valores);
            return prenda;
        }

        private static CrPrendaDetalleData MapearPrendaDetalle(dynamic row)
        {
            var valores = CrearDiccionario(row);
            var detalle = new CrPrendaDetalleData
            {
                color = ObtenerString(valores, "Color"),
                observaciones = ObtenerString(valores, "Observaciones", "OBSERVACIONES"),
                chasis_numero = ObtenerString(valores, "CHASIS_NUMERO"),
                vin_motor = ObtenerString(valores, "VIN_MOTOR"),
                puertas_numero = ObtenerDecimal(valores, "PUERTAS_NUMERO"),
                peso = ObtenerDecimal(valores, "Peso"),
                capacidad = ObtenerDecimal(valores, "Capacidad"),
                cilindraje = ObtenerDecimal(valores, "Cilindraje"),
                valor_fiscal = ObtenerDecimal(valores, "VALOR_FISCAL"),
                monto_extras = ObtenerDecimal(valores, "Monto_Extras"),
                avaluo_observacion = ObtenerString(valores, "AVALUO_OBSERVACION"),
                avaluo_inspeccion = ObtenerString(valores, "AVALUO_INSPECCION"),
                avaluo_inspector = ObtenerString(valores, "AVALUO_INSPECTOR"),
                poliza_mnt_formalizacion = ObtenerDecimal(valores, "POLIZA_MNT_FORMALIZACION"),
                poliza_rst_plan = ObtenerDecimal(valores, "POLIZA_RST_PLAN"),
                notario_registro_usuario = ObtenerString(valores, "NOTARIO_REGISTRO_USUARIO"),
                notario_actualiza_usuario = ObtenerString(valores, "NOTARIO_ACTUALIZA_USUARIO"),
                notario_actualiza_fecha = ObtenerString(valores, "NOTARIO_ACTUALIZA_FECHA"),
                combustible_desc = ObtenerString(valores, "COBUSTIBLE_DESC"),
                comercializa_desc = ObtenerString(valores, "COMERCIALIZA_DESC"),
                marca_desc = ObtenerString(valores, "MARCA_DESC"),
                modelo_desc = ObtenerString(valores, "MODELO_DESC"),
                presentacion_desc = ObtenerString(valores, "PRESENTACION_DESC"),
                id_combustible = ObtenerInt(valores, "ID_COMBUSTIBLE"),
                id_comercio = ObtenerInt(valores, "ID_COMERCIO"),
                id_marca = ObtenerInt(valores, "ID_MARCA"),
                id_modelo = ObtenerInt(valores, "ID_MODELO"),
                id_presentacion = ObtenerInt(valores, "ID_PRESENTACION"),
                peso_ud = ObtenerString(valores, "PESO_UD"),
                capacidad_ud = ObtenerString(valores, "CAPACIDAD_UD"),
                cilindraje_ud = ObtenerString(valores, "CILINDRAJE_UD"),
                peso_ud_desc = ObtenerString(valores, "PESO_UD_DESC"),
                capacidad_ud_desc = ObtenerString(valores, "CAPACIDAD_UD_DESC"),
                cilindraje_ud_desc = ObtenerString(valores, "CILINDRAJE_UD_DESC"),
                titular_nombre = ObtenerString(valores, "Titular_Nombre"),
                titular_tercero = ObtenerInt(valores, "Titular_Tercero")
            };

            CompletarPrendaBase(detalle, valores);
            CompletarPolizaExterna(detalle, valores);
            AsignarEstadoPoliza(detalle);

            return detalle;
        }

        private static void CompletarPrendaBase(CrPrendaListaData prenda, IDictionary<string, object> valores)
        {
            prenda.prenda_id = ObtenerLong(valores, ColPrendaId);
            prenda.tipo_prenda = ObtenerString(valores, "Tipo_Prenda");
            prenda.tipo_prenda_desc = ObtenerString(valores, "Tipo_Prenda_Desc", "PrendaDesc");
            prenda.avaluo = ObtenerDecimal(valores, "Avaluo");
            prenda.porc_cobertura = ObtenerDecimal(valores, "Porc_Cobertura");
            prenda.cobertura = ObtenerDecimal(valores, "Cobertura");
            prenda.descripcion = ObtenerString(valores, "Descripcion");
            prenda.id_principal = ObtenerString(valores, "ID_PRINCIPAL");
            prenda.id_provisional = ObtenerString(valores, "ID_PROVISIONAL");
            prenda.modelo = ObtenerString(valores, "Modelo");
            prenda.serie = ObtenerString(valores, "Serie");
            prenda.marca = ObtenerString(valores, "Marca");
            prenda.anio = ObtenerString(valores, "Anio");
            prenda.registro_fecha = ObtenerString(valores, ColRegistroFecha);
            prenda.registro_usuario = ObtenerString(valores, ColRegistroUsuario);
            prenda.actualiza_fecha = ObtenerString(valores, "Actualiza_Fecha", ColActualizaFecha);
            prenda.actualiza_usuario = ObtenerString(valores, "Actualiza_Usuario", ColActualizaUsuario);
            prenda.tomo = ObtenerString(valores, "Tomo");
            prenda.folio = ObtenerString(valores, "Folio");
            prenda.notario = ObtenerString(valores, "Notario");
            prenda.notario_registro_fecha = ObtenerString(valores, "NOTARIO_REGISTRO_FECHA");
        }

        private static void CompletarPolizaExterna(CrPrendaDetalleData detalle, IDictionary<string, object> valores)
        {
            detalle.pe_indica = ObtenerInt(valores, "PE_INDICA", "PolizaIndica", "POLIZA_INDICA");
            detalle.pe_id = ObtenerInt(valores, "PE_Id", "PE_ID");
            detalle.pe_numero = ObtenerString(valores, "PE_NUMERO");
            detalle.pe_prima = ObtenerDecimal(valores, "PE_PRIMA");
            detalle.pe_frecuencia = ObtenerString(valores, "PE_FRECUENCIA");
            detalle.pe_inicio = ObtenerString(valores, "PE_INICIO");
            detalle.pe_vence = ObtenerString(valores, "PE_VENCE");
            detalle.pe_activa = ObtenerInt(valores, "PE_ACTIVA");
            detalle.pe_cobertura = ObtenerString(valores, "PE_Cobertura", "PE_COBERTURA");
            detalle.pe_notas = ObtenerString(valores, "PE_NOTAS");
            detalle.aseguradora_desc = ObtenerString(valores, "ASEGURADORA_DESC");
            detalle.id_aseguradora = ObtenerInt(valores, "ID_ASEGURADORA");
            detalle.a_cedula = ObtenerString(valores, "A_CEDULA");
            detalle.a_tipo_id = ObtenerString(valores, "A_TIPO_ID", "A_TIPOID", "TIPO_ID");
            detalle.a_tipo_id_desc = ObtenerString(valores, "A_TIPO_ID_DESC", "A_TIPOID_DESC", "TIPO_ID_DESC");
            detalle.a_apellido_1 = ObtenerString(valores, "A_APELLIDO_1");
            detalle.a_apellido_2 = ObtenerString(valores, "A_APELLIDO_2");
            detalle.a_nombre = ObtenerString(valores, "A_NOMBRE");
            detalle.a_email = ObtenerString(valores, "A_EMAIL");
            detalle.a_tel_movil = ObtenerString(valores, "A_TEL_MOVIL");
            detalle.a_nacimiento = ObtenerString(valores, "A_NACIMIENTO");
            detalle.a_sexo = ObtenerString(valores, "A_SEXO");
            detalle.a_parentesco_desc = ObtenerString(valores, "A_PARENTESCO_DESC");
            detalle.a_cod_parentesco = ObtenerString(valores, "A_COD_PARENTESCO");
            detalle.pe_vencida = ObtenerInt(valores, "PE_VENCIDA");
        }

        private static void AsignarEstadoPoliza(CrPrendaDetalleData detalle)
        {
            if (detalle.pe_indica != 1)
            {
                return;
            }

            detalle.pe_status = detalle.pe_vencida == 1
                ? "Utiliza PÃ³liza Externa y se encuentra Vencida!"
                : "Utiliza PÃ³liza Externa!";
        }

        private static string NombreCompletoAsegurado(CrPrendaDetalleData detalle)
        {
            return string.Join(" ", new[]
            {
                detalle.a_apellido_1,
                detalle.a_apellido_2,
                detalle.a_nombre
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static CrPrendaAnotacionData MapearAnotacion(dynamic row)
        {
            var valores = CrearDiccionario(row);

            return new CrPrendaAnotacionData
            {
                id_nota = ObtenerLong(valores, "ID_NOTA"),
                notas = ObtenerString(valores, "Notas"),
                registro_fecha = ObtenerString(valores, ColRegistroFecha),
                registro_usuario = ObtenerString(valores, ColRegistroUsuario)
            };
        }

        private static CrPrendaPolizaCoberturaData MapearPolizaCobertura(dynamic row)
        {
            var valores = CrearDiccionario(row);

            return new CrPrendaPolizaCoberturaData
            {
                id_prenda_cobertura = ObtenerLong(valores, "ID_PRENDA_COBERTURA"),
                cobertura = ObtenerString(valores, "Cobertura"),
                asignado = ObtenerInt(valores, "asignado")
            };
        }

        private static CrPrendaHistoricoAvaluoData MapearHistoricoAvaluo(dynamic row)
        {
            var valores = CrearDiccionario(row);

            return new CrPrendaHistoricoAvaluoData
            {
                id_avaluo_h = ObtenerLong(valores, "ID_AVALUO_H"),
                prenda_id = ObtenerLong(valores, ColPrendaId),
                inspector = ObtenerString(valores, "INSPECTOR"),
                fecha_inspeccion = ObtenerString(valores, "FECHA_INSPECCION"),
                valor_mercado = ObtenerDecimal(valores, "VALOR_MERCADO"),
                valor_fiscal = ObtenerDecimal(valores, "VALOR_FISCAL"),
                observaciones = ObtenerString(valores, "OBSERVACIONES"),
                registro_fecha = ObtenerString(valores, ColRegistroFecha),
                registro_usuario = ObtenerString(valores, ColRegistroUsuario),
                actualiza_fecha = ObtenerString(valores, ColActualizaFecha),
                actualiza_usuario = ObtenerString(valores, ColActualizaUsuario)
            };
        }

        private static CrPrendaHistoricoPolizaData MapearHistoricoPoliza(dynamic row)
        {
            var valores = CrearDiccionario(row);
            var poliza = new CrPrendaDetalleData();

            CompletarPolizaExterna(poliza, valores);

            return new CrPrendaHistoricoPolizaData
            {
                pe_id = poliza.pe_id,
                prenda_id = ObtenerLong(valores, ColPrendaId),
                pe_numero = poliza.pe_numero,
                pe_activa = poliza.pe_activa,
                pe_frecuencia = poliza.pe_frecuencia,
                pe_prima = poliza.pe_prima,
                pe_inicio = poliza.pe_inicio,
                pe_vence = poliza.pe_vence,
                pe_vencida = poliza.pe_vencida,
                aseguradora_desc = poliza.aseguradora_desc,
                id_aseguradora = poliza.id_aseguradora,
                a_cedula = poliza.a_cedula,
                asegurado = NombreCompletoAsegurado(poliza),
                a_parentesco_desc = poliza.a_parentesco_desc,
                a_tel_movil = poliza.a_tel_movil,
                a_email = poliza.a_email,
                pe_cobertura = poliza.pe_cobertura,
                pe_notas = poliza.pe_notas,
                registro_fecha = ObtenerString(valores, ColRegistroFecha),
                registro_usuario = ObtenerString(valores, ColRegistroUsuario),
                actualiza_fecha = ObtenerString(valores, ColActualizaFecha),
                actualiza_usuario = ObtenerString(valores, ColActualizaUsuario)
            };
        }

        private static CrPrendaDetalleData MapearPolizaExternaDetalle(dynamic row)
        {
            var valores = CrearDiccionario(row);
            var detalle = new CrPrendaDetalleData();

            CompletarPolizaExterna(detalle, valores);

            if (detalle.pe_indica == 0 && detalle.pe_id > 0)
            {
                detalle.pe_indica = 1;
            }

            AsignarEstadoPoliza(detalle);

            return detalle;
        }

        private static IDictionary<string, object> CrearDiccionario(dynamic row)
            => new Dictionary<string, object>((IDictionary<string, object>)row, StringComparer.OrdinalIgnoreCase);

        private static string ObtenerString(IDictionary<string, object> valores, params string[] llaves)
        {
            return llaves
                .Where(llave => valores.TryGetValue(llave, out var valor) && valor != null)
                .Select(llave => Convert.ToString(valores[llave])?.Trim())
                .FirstOrDefault() ?? string.Empty;
        }

        private static long ObtenerLong(IDictionary<string, object> valores, string llave)
            => valores.TryGetValue(llave, out var valor) && valor != null ? Convert.ToInt64(valor) : 0;

        private static int ObtenerInt(IDictionary<string, object> valores, params string[] llaves)
        {
            foreach (var llave in llaves)
            {
                if (valores.TryGetValue(llave, out var valor) && valor != null)
                {
                    return Convert.ToInt32(valor);
                }
            }

            return 0;
        }

        private static decimal ObtenerDecimal(IDictionary<string, object> valores, string llave)
            => valores.TryGetValue(llave, out var valor) && valor != null ? Convert.ToDecimal(valor) : 0;

        private static string UsuarioNormalizado(string usuario)
            => (usuario ?? string.Empty).Trim().ToUpperInvariant();

        private static ErrorDto<string> CrearRespuestaMovimiento(
            dynamic resp,
            string mensajeSinRespuesta,
            Func<string, string> crearMensajeExito)
        {
            if (resp.Code == -1)
            {
                return new ErrorDto<string>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = string.Empty
                };
            }

            if (resp.Result == null)
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = mensajeSinRespuesta,
                    Result = string.Empty
                };
            }

            var valores = CrearDiccionario(resp.Result);
            var pass = ObtenerInt(valores, "Pass");
            var mensaje = ObtenerString(valores, "Mensaje");
            var movimiento = ObtenerString(valores, "Movimiento");

            return new ErrorDto<string>
            {
                Code = pass == 1 ? 0 : -1,
                Description = pass == 1 ? string.Empty : mensaje,
                Result = pass == 1 ? crearMensajeExito(movimiento) : mensaje
            };
        }

        private static ErrorDto<string> RespuestaSp(dynamic resp, string mensajeExito)
        {
            if (resp.Code == -1)
            {
                return new ErrorDto<string>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = string.Empty
                };
            }

            if (resp.Result == null)
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "No se obtuvo respuesta del procedimiento.",
                    Result = string.Empty
                };
            }

            var valores = CrearDiccionario(resp.Result);
            var pass = ObtenerInt(valores, "Pass");
            var mensaje = ObtenerString(valores, "Mensaje");

            return new ErrorDto<string>
            {
                Code = pass == 1 ? 0 : -1,
                Description = pass == 1 ? string.Empty : mensaje,
                Result = pass == 1 ? mensajeExito : mensaje
            };
        }

        private static int ObtenerEnteroSeguro(string valor)
            => int.TryParse((valor ?? string.Empty).Trim(), out var numero) ? numero : 0;

        private static string ObtenerInicial(string valor)
        {
            var texto = (valor ?? string.Empty).Trim();
            return texto.Length == 0 ? string.Empty : texto.Substring(0, 1).ToUpperInvariant();
        }

        private static string ConvertirFechaSql(string valor)
        {
            var texto = (valor ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            var partes = texto.Split('/');
            if (partes.Length == 3
                && int.TryParse(partes[0], out var dia)
                && int.TryParse(partes[1], out var mes)
                && int.TryParse(partes[2], out var anio))
            {
                return new DateTime(anio, mes, dia, 0, 0, 0, DateTimeKind.Unspecified)
                    .ToString(FormatoFechaSql, CultureInfo.InvariantCulture);
            }

            return DateTime.TryParse(
                texto,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha)
                ? fecha.ToString(FormatoFechaSql, CultureInfo.InvariantCulture)
                : texto;
        }
    }
}

