using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoCargosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string guardadoExisto = "Informacion guardada satisfactoriamente...";
        private const string eliminadoExisto = "Informacion eliminada satisfactoriamente...";

        public FrmCrCatalogoCargosDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoCargosDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista principal de cargos adicionales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCargoData>> CrCatalogoCargos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select 
                cod_cargo,descripcion,automatico,AUMENTA_BASE_CRD,
                base as base_calculo,tipo,valor,cod_cuenta,tipo_deduccion
                ,plazo_tipo,plazo_dias,monto_inicio,monto_corte,
                diferido_cargo,diferido_cod_cuenta
                ,iva_porcentaje, activo
                from cargos_adicionales
                order by cod_cargo";

            return DbHelper.ExecuteListQuery<CrCatalogoCargoData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Guarda un cargo adicional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCargos_Guardar(
            int codEmpresa,
            CrCatalogoCargoGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cargo.cod_cargo = Limpiar(request.cargo.cod_cargo);
            request.cargo.descripcion = Limpiar(request.cargo.descripcion);
            request.cargo.base_calculo = Limpiar(request.cargo.base_calculo);
            request.cargo.tipo = Limpiar(request.cargo.tipo);
            request.cargo.cod_cuenta = CuentaSinFormato(request.cargo.cod_cuenta);
            request.cargo.tipo_deduccion = Limpiar(request.cargo.tipo_deduccion);
            request.cargo.plazo_tipo = Limpiar(request.cargo.plazo_tipo);
            request.cargo.diferido_cod_cuenta = CuentaSinFormato(request.cargo.diferido_cod_cuenta);

            return ExisteCargo(codEmpresa, request.cargo.cod_cargo)
                ? ActualizarCargo(codEmpresa, request)
                : InsertarCargo(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un cargo adicional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCargos_Eliminar(
            int codEmpresa,
            CrCatalogoCargoEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_cargo = Limpiar(request.cod_cargo);

            if (string.IsNullOrWhiteSpace(request.cod_cargo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del cargo."
                };
            }

            const string sqlDelete = @"
                delete from CRD_CARGOS_ASG_DETALLE
                where cod_cargo = @CodCargo;

                delete from CRD_CARGOS_ADICIONAL_TABLA
                where cod_cargo = @CodCargo;

                delete from cargos_asignacion
                where cod_cargo = @CodCargo;

                delete from cargos_adicionales
                where cod_cargo = @CodCargo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { CodCargo = request.cod_cargo }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Cargo Adicional Cod: {request.cod_cargo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoExisto
            };
        }

        /// <summary>
        /// Obtiene el arbol de lineas, destinos y garantias para asignacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCargoArbolData>> CrCatalogoCargos_AsignacionArbol_Obtener(
            int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    X.[key],
                    X.parent_key,
                    X.label,
                    X.leaf,
                    X.codigo,
                    X.cod_destino,
                    X.garantia,
                    X.nivel
                from
                (
                    select
                        '0x0' + rtrim(C.codigo) + 'L' as [key],
                        '' as parent_key,
                        rtrim(C.codigo) + ' - ' + rtrim(C.descripcion) as label,
                        cast(0 as bit) as leaf,
                        rtrim(C.codigo) as codigo,
                        '' as cod_destino,
                        '' as garantia,
                        'L' as nivel,
                        1 as orden_nivel,
                        rtrim(C.codigo) as orden_codigo,
                        '' as orden_destino,
                        '' as orden_garantia
                    from catalogo C
                    where C.retencion = 'N'
                      and C.poliza = 'N'
                      and C.activo = 1

                    union all

                    select
                        '0x0' + rtrim(A.codigo) + '-' + rtrim(A.cod_destino) + 'D' as [key],
                        '0x0' + rtrim(A.codigo) + 'L' as parent_key,
                        rtrim(D.cod_destino) + ' - ' + rtrim(D.descripcion) as label,
                        cast(0 as bit) as leaf,
                        rtrim(A.codigo) as codigo,
                        rtrim(A.cod_destino) as cod_destino,
                        '' as garantia,
                        'D' as nivel,
                        2 as orden_nivel,
                        rtrim(A.codigo) as orden_codigo,
                        rtrim(A.cod_destino) as orden_destino,
                        '' as orden_garantia
                    from CATALOGO_DESTINOSASG A
                    inner join catalogo_destinos D
                        on D.cod_destino = A.cod_destino

                    union all

                    select distinct
                        '0x0' + rtrim(A.codigo) + '-' + rtrim(A.cod_destino) + 'D-' + rtrim(G.garantia) + 'G' as [key],
                        '0x0' + rtrim(A.codigo) + '-' + rtrim(A.cod_destino) + 'D' as parent_key,
                        rtrim(T.descripcion) as label,
                        cast(1 as bit) as leaf,
                        rtrim(A.codigo) as codigo,
                        rtrim(A.cod_destino) as cod_destino,
                        rtrim(G.garantia) as garantia,
                        'G' as nivel,
                        3 as orden_nivel,
                        rtrim(A.codigo) as orden_codigo,
                        rtrim(A.cod_destino) as orden_destino,
                        rtrim(G.garantia) as orden_garantia
                    from CATALOGO_DESTINOSASG A
                    inner join crd_catalogo_garantias G
                        on G.codigo = A.codigo
                    inner join crd_garantia_tipos T
                        on T.garantia = G.garantia
                ) X
                order by
                    X.orden_codigo,
                    X.orden_nivel,
                    X.orden_destino,
                    X.orden_garantia;";

            return DbHelper.ExecuteListQuery<CrCatalogoCargoArbolData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Obtiene los cargos asignables para una combinacion linea-destino-garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCargoAsignacionData>> CrCatalogoCargos_AsignacionCargos_Obtener(
            int codEmpresa,
            CrCatalogoCargoAsignacionObtenerRequest request)
        {
            const string sqlQuery = @"
                select
                    rtrim(R.cod_cargo) as cod_cargo,
                    rtrim(isnull(R.descripcion, '')) as descripcion,
                    rtrim(isnull(R.tipo, '')) as tipo,
                    isnull(R.valor, 0) as valor,
                    cast(case when A.cod_cargo is null then 0 else 1 end as bit) as existe
                from Cargos_Adicionales R
                left join CRD_CARGOS_ASG_DETALLE A
                    on R.cod_cargo = A.cod_cargo
                   and A.codigo = @Codigo
                   and A.cod_destino = @CodDestino
                   and A.garantia = @Garantia
                order by existe desc, R.cod_cargo;";

            return DbHelper.ExecuteListQuery<CrCatalogoCargoAsignacionData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new
                {
                    Codigo = Limpiar(request.codigo),
                    CodDestino = Limpiar(request.cod_destino),
                    Garantia = Limpiar(request.garantia)
                }
            );
        }

        /// <summary>
        /// Guarda o elimina la asignacion de un cargo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCargos_Asignacion_Guardar(
            int codEmpresa,
            CrCatalogoCargoAsignacionGuardarRequest request)
        {
            request.cod_cargo = Limpiar(request.cod_cargo);
            request.codigo = Limpiar(request.codigo);
            request.cod_destino = Limpiar(request.cod_destino);
            request.garantia = Limpiar(request.garantia);

            if (string.IsNullOrWhiteSpace(request.cod_cargo) ||
                string.IsNullOrWhiteSpace(request.codigo) ||
                string.IsNullOrWhiteSpace(request.cod_destino) ||
                string.IsNullOrWhiteSpace(request.garantia))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el cargo, linea, destino y garantia."
                };
            }

            string sql = request.isChecked
                ? @"
                if not exists
                (
                    select 1
                    from CRD_CARGOS_ASG_DETALLE
                    where cod_cargo = @CodCargo
                      and codigo = @Codigo
                      and cod_destino = @CodDestino
                      and garantia = @Garantia
                )
                begin
                    insert into CRD_CARGOS_ASG_DETALLE(cod_cargo, codigo, cod_destino, garantia)
                    values(@CodCargo, @Codigo, @CodDestino, @Garantia)
                end"
                : @"
                delete from CRD_CARGOS_ASG_DETALLE
                where cod_cargo = @CodCargo
                  and codigo = @Codigo
                  and cod_destino = @CodDestino
                  and garantia = @Garantia;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodCargo = request.cod_cargo,
                    Codigo = request.codigo,
                    CodDestino = request.cod_destino,
                    Garantia = request.garantia
                }
            );
        }

        /// <summary>
        /// Obtiene los cargos tipo tabla activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogoCargos_TablaAplicacionCargos_Obtener(
            int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_CARGO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CARGOS_ADICIONALES
                where TIPO = 'T'
                  and ACTIVO = 1
                order by COD_CARGO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Obtiene la tabla de aplicacion del cargo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCargoTablaAplicacionData>> CrCatalogoCargos_TablaAplicacion_Obtener(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionObtenerRequest request)
        {
            const string sqlQuery = @"
                select
                    ID_TABLA as id_tabla,
                    MONTO_INICIO as monto_inicio,
                    MONTO_CORTE as monto_corte,
                    PLAZO_INICIO as plazo_inicio,
                    PLAZO_CORTE as plazo_corte,
                    rtrim(isnull(APL_TIPO, '')) as tipo,
                    APL_VALOR as apl_valor
                from CRD_CARGOS_ADICIONAL_TABLA
                where COD_CARGO = @CodCargo
                order by MONTO_INICIO;";

            return DbHelper.ExecuteListQuery<CrCatalogoCargoTablaAplicacionData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new { CodCargo = Limpiar(request.cod_cargo) }
            );
        }

        /// <summary>
        /// Guarda una fila de tabla de aplicacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCargos_TablaAplicacion_Guardar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_cargo = Limpiar(request.cod_cargo);
            request.tipo = Limpiar(request.tipo);

            if (request.id_tabla <= 0)
            {
                int siguienteId = ObtenerSiguienteIdTabla(codEmpresa, request.cod_cargo);

                const string sqlInsert = @"
                    insert into CRD_CARGOS_ADICIONAL_TABLA
                    (
                        cod_cargo,
                        ID_TABLA,
                        MONTO_INICIO,
                        MONTO_CORTE,
                        PLAZO_INICIO,
                        PLAZO_CORTE,
                        APL_TIPO,
                        APL_VALOR,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO
                    )
                    values
                    (
                        @CodCargo,
                        @IdTabla,
                        @MontoInicio,
                        @MontoCorte,
                        @PlazoInicio,
                        @PlazoCorte,
                        @Tipo,
                        @AplValor,
                        dbo.MyGetdate(),
                        @Usuario
                    );";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        CodCargo = request.cod_cargo,
                        IdTabla = siguienteId,
                        MontoInicio = request.monto_inicio,
                        MontoCorte = request.monto_corte,
                        PlazoInicio = request.plazo_inicio,
                        PlazoCorte = request.plazo_corte,
                        Tipo = request.tipo,
                        AplValor = request.apl_valor,
                        Usuario = request.usuario
                    }
                );

                if (respInsert.Code < 0)
                    return respInsert;

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Registra - WEB",
                    $"Cargo Adicional (Tabla) Cod: {request.cod_cargo}, Id.{siguienteId}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = guardadoExisto
                };
            }

            const string sqlUpdate = @"
                update CRD_CARGOS_ADICIONAL_TABLA
                set MONTO_INICIO = @MontoInicio,
                    MONTO_CORTE = @MontoCorte,
                    PLAZO_INICIO = @PlazoInicio,
                    PLAZO_CORTE = @PlazoCorte,
                    APL_TIPO = @Tipo,
                    APL_VALOR = @AplValor
                where COD_CARGO = @CodCargo
                  and ID_TABLA = @IdTabla;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodCargo = request.cod_cargo,
                    IdTabla = request.id_tabla,
                    MontoInicio = request.monto_inicio,
                    MontoCorte = request.monto_corte,
                    PlazoInicio = request.plazo_inicio,
                    PlazoCorte = request.plazo_corte,
                    Tipo = request.tipo,
                    AplValor = request.apl_valor
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Actualiza - WEB",
                $"Cargo Adicional (Tabla) Cod: {request.cod_cargo}, Id.{request.id_tabla}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExisto
            };
        }

        /// <summary>
        /// Elimina una fila de tabla de aplicacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCargos_TablaAplicacion_Eliminar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_cargo = Limpiar(request.cod_cargo);

            const string sqlDelete = @"
                delete from CRD_CARGOS_ADICIONAL_TABLA
                where COD_CARGO = @CodCargo
                  and ID_TABLA = @IdTabla;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodCargo = request.cod_cargo,
                    IdTabla = request.id_tabla
                }
            );

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Cargo Adicional (Tabla) Cod: {request.cod_cargo}, Id.{request.id_tabla}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoExisto
            };
        }

        private ErrorDto InsertarCargo(int codEmpresa, CrCatalogoCargoGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into cargos_adicionales
                (
                    cod_cargo,
                    descripcion,
                    automatico,
                    AUMENTA_BASE_CRD,
                    base,
                    tipo,
                    valor,
                    cod_cuenta,
                    tipo_deduccion,
                    plazo_tipo,
                    plazo_dias,
                    monto_inicio,
                    monto_corte,
                    diferido_cargo,
                    diferido_cod_cuenta,
                    iva_porcentaje,
                    activo
                )
                values
                (
                    @CodCargo,
                    @Descripcion,
                    @Automatico,
                    @AumentaBaseCrd,
                    @BaseCalculo,
                    @Tipo,
                    @Valor,
                    @CodCuenta,
                    @TipoDeduccion,
                    @PlazoTipo,
                    @PlazoDias,
                    @MontoInicio,
                    @MontoCorte,
                    @DiferidoCargo,
                    @DiferidoCodCuenta,
                    @IvaPorcentaje,
                    @Activo
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodCargo = request.cargo.cod_cargo,
                    Descripcion = request.cargo.descripcion,
                    Automatico = request.cargo.automatico ? 1 : 0,
                    AumentaBaseCrd = request.cargo.aumenta_base_crd ? 1 : 0,
                    BaseCalculo = request.cargo.base_calculo,
                    Tipo = request.cargo.tipo,
                    Valor = request.cargo.valor,
                    CodCuenta = request.cargo.cod_cuenta,
                    TipoDeduccion = request.cargo.tipo_deduccion,
                    PlazoTipo = request.cargo.plazo_tipo,
                    PlazoDias = request.cargo.plazo_dias < 0 ? 0 : request.cargo.plazo_dias,
                    MontoInicio = request.cargo.monto_inicio,
                    MontoCorte = request.cargo.monto_corte,
                    DiferidoCargo = request.cargo.diferido_cargo ? 1 : 0,
                    DiferidoCodCuenta = request.cargo.diferido_cargo ? request.cargo.diferido_cod_cuenta : string.Empty,
                    IvaPorcentaje = request.cargo.iva_porcentaje,
                    Activo = request.cargo.activo ? 1 : 0
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Cargo Adicional Cod: {request.cargo.cod_cargo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExisto
            };
        }

        private ErrorDto ActualizarCargo(int codEmpresa, CrCatalogoCargoGuardarRequest request)
        {
            const string sqlUpdate = @"
                update cargos_adicionales
                set descripcion = @Descripcion,
                    automatico = @Automatico,
                    AUMENTA_BASE_CRD = @AumentaBaseCrd,
                    base = @BaseCalculo,
                    tipo = @Tipo,
                    valor = @Valor,
                    cod_cuenta = @CodCuenta,
                    tipo_deduccion = @TipoDeduccion,
                    plazo_tipo = @PlazoTipo,
                    plazo_dias = @PlazoDias,
                    monto_inicio = @MontoInicio,
                    monto_corte = @MontoCorte,
                    diferido_cargo = @DiferidoCargo,
                    diferido_cod_cuenta = @DiferidoCodCuenta,
                    iva_porcentaje = @IvaPorcentaje,
                    activo = @Activo
                where cod_cargo = @CodCargo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodCargo = request.cargo.cod_cargo,
                    Descripcion = request.cargo.descripcion,
                    Automatico = request.cargo.automatico ? 1 : 0,
                    AumentaBaseCrd = request.cargo.aumenta_base_crd ? 1 : 0,
                    BaseCalculo = request.cargo.base_calculo,
                    Tipo = request.cargo.tipo,
                    Valor = request.cargo.valor,
                    CodCuenta = request.cargo.cod_cuenta,
                    TipoDeduccion = request.cargo.tipo_deduccion,
                    PlazoTipo = request.cargo.plazo_tipo,
                    PlazoDias = request.cargo.plazo_dias < 0 ? 0 : request.cargo.plazo_dias,
                    MontoInicio = request.cargo.monto_inicio,
                    MontoCorte = request.cargo.monto_corte,
                    DiferidoCargo = request.cargo.diferido_cargo ? 1 : 0,
                    DiferidoCodCuenta = request.cargo.diferido_cargo ? request.cargo.diferido_cod_cuenta : string.Empty,
                    IvaPorcentaje = request.cargo.iva_porcentaje,
                    Activo = request.cargo.activo ? 1 : 0
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Cargo Adicional Cod: {request.cargo.cod_cargo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExisto
            };
        }

        private bool ExisteCargo(int codEmpresa, string codCargo)
        {
            const string sqlQuery = @"
                select isnull(count(*), 0)
                from cargos_adicionales
                where cod_cargo = @CodCargo;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                0,
                new { CodCargo = codCargo }
            );

            return resp.Result > 0;
        }

        private bool CuentaExiste(int codEmpresa, string cuenta)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
                return false;

            const string sqlQuery = @"
                select isnull(count(*), 0)
                from vCNTX_CUENTAS_LOCAL
                where cod_cuenta = @Cuenta;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                0,
                new { Cuenta = cuenta }
            );

            return resp.Result > 0;
        }

        private int ObtenerSiguienteIdTabla(int codEmpresa, string codCargo)
        {
            const string sqlQuery = @"
                select isnull(max(ID_TABLA), 0) + 1
                from CRD_CARGOS_ADICIONAL_TABLA
                where COD_CARGO = @CodCargo;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                1,
                new { CodCargo = codCargo }
            );

            return resp.Result <= 0 ? 1 : resp.Result;
        }

        private static string Limpiar(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string CuentaSinFormato(string cuenta)
        {
            return (cuenta ?? string.Empty)
                .Replace("-", string.Empty)
                .Trim();
        }

        private static bool EsUnoDe(string valor, params string[] permitidos)
        {
            return permitidos.Contains(Limpiar(valor));
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}