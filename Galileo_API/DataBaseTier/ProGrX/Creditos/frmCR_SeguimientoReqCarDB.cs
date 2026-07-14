using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoReqCarDb
    {
        private const string MsgOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MsgCodigoRequerido = "Debe indicar el c&oacute;digo de l&iacute;nea.";
        private const string MsgCargoRequerido = "Debe indicar el cargo.";
        private const string MsgUsuarioRequerido = "Debe indicar el usuario.";
        private const string MsgMontoInvalido = "Debe indicar un monto v&aacute;lido.";
        private const string MsgReqGuardar = "No fue posible guardar los requisitos.";
        private const string MsgCargoAplicar = "No fue posible aplicar el cargo.";
        private const string MsgPrimaGuardar = "No fue posible guardar la prima.";
        private const string MsgOperacionNoExiste = "No se encontr&oacute; la operaci&oacute;n indicada.";
        private const string MsgOperacionBloqueada = "La operaci&oacute;n ya se encuentra anulada o formalizada y no permite modificaciones.";
        private const string MsgPrimasObtener = "No fue posible consultar las primas.";

        private const string VentanaRequisitos = "R";
        private const string VentanaCargos = "C";
        private const string TipoConsultaManual = "M";
        private const string TipoConsultaRegistrado = "R";

        private readonly PortalDB _portalDb;

        public FrmCrSeguimientoReqCarDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la informacion base de la operacion y define el modo de la pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoReqCarCargaInicialData> CrSeguimientoReqCar_CargaInicial_Obtener(
            int codEmpresa,
            CrSeguimientoReqCarCargaInicialRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargaInicialData>(
                    MsgOperacionRequerida,
                    -2,
                    new CrSeguimientoReqCarCargaInicialData());
            }

            var operacionResponse = ObtenerOperacion(codEmpresa, request.operacion);
            if (operacionResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargaInicialData>(
                    operacionResponse.Description ?? MsgOperacionNoExiste,
                    operacionResponse.Code ?? -1,
                    new CrSeguimientoReqCarCargaInicialData());
            }

            var operacion = operacionResponse.Result ?? new CrSeguimientoReqCarOperacionData();
            if (operacion.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargaInicialData>(
                    MsgOperacionNoExiste,
                    -2,
                    new CrSeguimientoReqCarCargaInicialData());
            }

            string ventana = NormalizarVentana(request.ventana);
            bool editable = EsEditable(operacion.estado_solicitud);

            var data = new CrSeguimientoReqCarCargaInicialData
            {
                operacion = operacion.operacion,
                codigo = operacion.codigo,
                identificacion = operacion.identificacion,
                nombre = operacion.nombre,
                ventana = ventana,
                estado_solicitud = operacion.estado_solicitud,
                editable = editable
            };

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Obtiene el listado de requisitos visibles de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrSeguimientoReqCarRequisitoData>> CrSeguimientoReqCar_Requisitos_Obtener(
            int codEmpresa,
            CrSeguimientoReqCarRequisitosRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoReqCarRequisitoData>>(
                    MsgOperacionRequerida,
                    -2,
                    new List<CrSeguimientoReqCarRequisitoData>());
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrSeguimientoReqCarRequisitoData>(
                    "exec spCrdRequisitosOperacionLista @Operacion",
                    new { Operacion = request.operacion }).ToList());
        }

        /// <summary>
        /// Obtiene los cargos adicionales y las primas de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoReqCarCargosData> CrSeguimientoReqCar_Cargos_Obtener(
            int codEmpresa,
            CrSeguimientoReqCarCargosRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargosData>(
                    MsgOperacionRequerida,
                    -2,
                    new CrSeguimientoReqCarCargosData());
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargosData>(
                    MsgCodigoRequerido,
                    -2,
                    new CrSeguimientoReqCarCargosData());
            }

            var operacionResponse = ObtenerOperacion(codEmpresa, request.operacion);
            if (operacionResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargosData>(
                    operacionResponse.Description ?? MsgOperacionNoExiste,
                    operacionResponse.Code ?? -1,
                    new CrSeguimientoReqCarCargosData());
            }

            var operacion = operacionResponse.Result ?? new CrSeguimientoReqCarOperacionData();
            if (operacion.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargosData>(
                    MsgOperacionNoExiste,
                    -2,
                    new CrSeguimientoReqCarCargosData());
            }

            string tipoConsulta = NormalizarTipoConsulta(request.tipo_consulta);
            List<CrSeguimientoReqCarCargoData> cargos = tipoConsulta == TipoConsultaManual
                ? ObtenerCargosManuales(codEmpresa, operacion)
                : ObtenerCargosRegistrados(codEmpresa, operacion.operacion);

            var primasResponse = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrSeguimientoReqCarPrimaData>(
                    @"
                    select
                        R.cod_cargo,
                        R.descripcion,
                        isnull(A.monto, 0) as monto
                    from cargos_adicionales R
                    left join operacion_cargos A
                        on R.cod_cargo = A.cod_cargo
                       and A.id_solicitud = @Operacion
                    where R.Base = 'P' OR R.tipo = 'P'
                    order by R.cod_cargo",
                    new { Operacion = operacion.operacion }).ToList());

            if (primasResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoReqCarCargosData>(
                    primasResponse.Description ?? MsgPrimasObtener,
                    primasResponse.Code ?? -1,
                    new CrSeguimientoReqCarCargosData());
            }

            var data = new CrSeguimientoReqCarCargosData
            {
                operacion = operacion.operacion,
                codigo = operacion.codigo,
                tipo_consulta = tipoConsulta,
                editable = EsEditable(operacion.estado_solicitud),
                cargos = cargos,
                primas = primasResponse.Result ?? new List<CrSeguimientoReqCarPrimaData>()
            };

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Guarda el estado de requisitos visibles y reprocesa los no visibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrSeguimientoReqCar_Requisitos_Guardar(
            int codEmpresa,
            CrSeguimientoReqCarRequisitosGuardarRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse(MsgCodigoRequerido, -2);
            }

            var operacionResponse = ObtenerOperacion(codEmpresa, request.operacion);
            if (operacionResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    operacionResponse.Description ?? MsgOperacionNoExiste,
                    operacionResponse.Code ?? -1);
            }

            var operacion = operacionResponse.Result ?? new CrSeguimientoReqCarOperacionData();
            if (operacion.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionNoExiste, -2);
            }

            if (!EsEditable(operacion.estado_solicitud))
            {
                return DbHelper.ErrorResponse(MsgOperacionBloqueada, -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                conn.Execute(
                    @"
                    delete operacion_requisitos
                    where id_solicitud = @Operacion
                      and cod_requisito in
                      (
                          select cod_requisito
                          from requisitos_adicionales
                          where visible = 1
                      )",
                    new { Operacion = request.operacion },
                    tx);

                foreach (var item in request.requisitos ?? new List<CrSeguimientoReqCarRequisitoGuardarItem>())
                {
                    if (string.IsNullOrWhiteSpace(item.cod_requisito))
                    {
                        continue;
                    }

                    conn.Execute(
                        @"
                        insert into operacion_requisitos
                        (
                            cod_requisito,
                            id_solicitud,
                            codigo,
                            estado,
                            opcional
                        )
                        values
                        (
                            @CodRequisito,
                            @Operacion,
                            @Codigo,
                            @Estado,
                            @Opcional
                        )",
                        new
                        {
                            CodRequisito = item.cod_requisito.Trim(),
                            Operacion = request.operacion,
                            Codigo = request.codigo.Trim(),
                            Estado = item.estado,
                            Opcional = item.opcional
                        },
                        tx);
                }

                conn.Execute(
                    "exec spCrdRequisitosOperacionNoVisiblesEstado @Operacion",
                    new { Operacion = request.operacion },
                    tx);

                tx.Commit();
                return DbHelper.OkResponse("OK");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"{MsgReqGuardar} {ex.Message}", -1);
            }
        }

        /// <summary>
        /// Inserta o elimina un cargo manual de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrSeguimientoReqCar_Cargo_Aplicar(
            int codEmpresa,
            CrSeguimientoReqCarCargoAplicarRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse(MsgCodigoRequerido, -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_cargo))
            {
                return DbHelper.ErrorResponse(MsgCargoRequerido, -2);
            }

            var operacionResponse = ObtenerOperacion(codEmpresa, request.operacion);
            if (operacionResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    operacionResponse.Description ?? MsgOperacionNoExiste,
                    operacionResponse.Code ?? -1);
            }

            var operacion = operacionResponse.Result ?? new CrSeguimientoReqCarOperacionData();
            if (operacion.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionNoExiste, -2);
            }

            if (!EsEditable(operacion.estado_solicitud))
            {
                return DbHelper.ErrorResponse(MsgOperacionBloqueada, -2);
            }

            string tipo = NormalizarTipoCargo(request.tipo);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                conn.Execute(
                    @"
                    delete operacion_cargos
                    where id_solicitud = @Operacion
                      and cod_cargo = @CodCargo",
                    new
                    {
                        Operacion = request.operacion,
                        CodCargo = request.cod_cargo.Trim()
                    },
                    tx);

                if (request.checked_ind)
                {
                    conn.Execute(
                        @"
                        insert into operacion_cargos
                        (
                            cod_cargo,
                            id_solicitud,
                            codigo,
                            tipo,
                            monto,
                            valor,
                            plazo_tipo,
                            plazo_dias,
                            tipo_deduccion,
                            diferido
                        )
                        values
                        (
                            @CodCargo,
                            @Operacion,
                            @Codigo,
                            @Tipo,
                            @Monto,
                            @Valor,
                            @PlazoTipo,
                            @PlazoDias,
                            'F',
                            @Diferido
                        )",
                        new
                        {
                            CodCargo = request.cod_cargo.Trim(),
                            Operacion = request.operacion,
                            Codigo = request.codigo.Trim(),
                            Tipo = tipo,
                            Monto = request.monto,
                            Valor = request.valor,
                            PlazoTipo = request.plazo_tipo.Trim(),
                            PlazoDias = request.plazo_dias,
                            Diferido = request.diferido_cargo ? 1 : 0
                        },
                        tx);
                }

                tx.Commit();
                return DbHelper.OkResponse("OK");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"{MsgCargoAplicar} {ex.Message}", -1);
            }
        }

        /// <summary>
        /// Guarda la prima de recargo por adquisicion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrSeguimientoReqCar_Prima_Guardar(
            int codEmpresa,
            CrSeguimientoReqCarPrimaGuardarRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_cargo))
            {
                return DbHelper.ErrorResponse(MsgCargoRequerido, -2);
            }

            if (request.monto < 0)
            {
                return DbHelper.ErrorResponse(MsgMontoInvalido, -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(MsgUsuarioRequerido, -2);
            }

            var operacionResponse = ObtenerOperacion(codEmpresa, request.operacion);
            if (operacionResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    operacionResponse.Description ?? MsgOperacionNoExiste,
                    operacionResponse.Code ?? -1);
            }

            var operacion = operacionResponse.Result ?? new CrSeguimientoReqCarOperacionData();
            if (operacion.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionNoExiste, -2);
            }

            if (!EsEditable(operacion.estado_solicitud))
            {
                return DbHelper.ErrorResponse(MsgOperacionBloqueada, -2);
            }

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                "exec spCrdOperacionFormalizaPrima @Operacion, @CodCargo, @Monto, @Usuario",
                new
                {
                    Operacion = request.operacion,
                    CodCargo = request.cod_cargo.Trim(),
                    Monto = request.monto,
                    Usuario = request.usuario.Trim()
                });

            return response.Code == 0
                ? DbHelper.OkResponse("OK")
                : DbHelper.ErrorResponse($"{MsgPrimaGuardar} {response.Description}", response.Code ?? -1);
        }

        private ErrorDto<CrSeguimientoReqCarOperacionData?> ObtenerOperacion(int codEmpresa, int operacion)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CrSeguimientoReqCarOperacionData>(
                    @"
                    select top 1
                        R.id_solicitud as operacion,
                        isnull(R.codigo, '') as codigo,
                        isnull(R.cedula, '') as identificacion,
                        isnull(S.nombre, '') as nombre,
                        isnull(R.estadosol, '') as estado_solicitud,
                        isnull(R.cod_destino, '') as cod_destino,
                        isnull(R.garantia, '') as garantia
                    from reg_creditos R
                    inner join socios S
                        on R.cedula = S.cedula
                    where R.id_solicitud = @Operacion",
                    new { Operacion = operacion }));
        }

        private List<CrSeguimientoReqCarCargoData> ObtenerCargosManuales(
            int codEmpresa,
            CrSeguimientoReqCarOperacionData operacion)
        {
            var asignadosResponse = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrSeguimientoReqCarCargoData>(
                    @"
                    select
                        R.cod_cargo,
                        R.descripcion,
                        isnull(dbo.fxCRDOperacionCargoValor(A.id_solicitud, R.cod_cargo, Getdate()), 0) as monto,
                        isnull(A.tipo, '') as tipo,
                        case when isnull(A.tipo, '') = 'P' then 'Porcentaje' else 'Monto' end as tipo_desc,
                        isnull(A.valor, 0) as valor,
                        isnull(R.plazo_tipo, '') as plazo_tipo,
                        isnull(R.plazo_dias, 0) as plazo_dias,
                        cast(isnull(R.Diferido_Cargo, 0) as bit) as diferido_cargo,
                        cast(1 as bit) as checked_ind
                    from cargos_adicionales R
                    inner join operacion_cargos A
                        on R.cod_cargo = A.cod_cargo
                       and A.id_solicitud = @Operacion
                    where R.Automatico = 0
                      and R.Base in ('C', 'A')
                    order by R.cod_cargo",
                    new { Operacion = operacion.operacion }).ToList());

            var resultado = asignadosResponse.Result ?? new List<CrSeguimientoReqCarCargoData>();

            var disponiblesResponse = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrSeguimientoReqCarCargoData>(
                    @"
                    select
                        C.cod_cargo,
                        C.descripcion,
                        isnull(dbo.fxCRDOperacionCargoValor(@Operacion, C.cod_cargo, Getdate()), 0) as monto,
                        isnull(C.tipo, '') as tipo,
                        case when isnull(C.tipo, '') = 'P' then 'Porcentaje' else 'Monto' end as tipo_desc,
                        isnull(C.valor, 0) as valor,
                        isnull(C.plazo_tipo, '') as plazo_tipo,
                        isnull(C.plazo_dias, 0) as plazo_dias,
                        cast(isnull(C.Diferido_Cargo, 0) as bit) as diferido_cargo,
                        cast(0 as bit) as checked_ind
                    from cargos_adicionales C
                    where C.cod_cargo in
                    (
                        select D.cod_cargo
                        from CRD_CARGOS_ASG_DETALLE D
                        where D.codigo = @Codigo
                          and D.cod_destino = @CodDestino
                          and D.garantia = @Garantia
                          and D.cod_cargo not in
                          (
                              select O.cod_cargo
                              from operacion_cargos O
                              where O.id_solicitud = @Operacion
                          )
                    )
                      and C.Automatico = 0
                      and C.Base in ('C', 'A')
                    order by C.cod_cargo",
                    new
                    {
                        Operacion = operacion.operacion,
                        Codigo = operacion.codigo,
                        CodDestino = operacion.cod_destino,
                        Garantia = operacion.garantia
                    }).ToList());

            if (disponiblesResponse.Result is not null && disponiblesResponse.Result.Count > 0)
            {
                resultado.AddRange(disponiblesResponse.Result);
            }

            return resultado;
        }

        private List<CrSeguimientoReqCarCargoData> ObtenerCargosRegistrados(int codEmpresa, int operacion)
        {
            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrSeguimientoReqCarCargoData>(
                    @"
                    select
                        R.cod_cargo,
                        R.descripcion,
                        isnull(A.monto, 0) as monto,
                        isnull(A.tipo, '') as tipo,
                        case when isnull(A.tipo, '') = 'P' then 'Porcentaje' else 'Monto' end as tipo_desc,
                        isnull(A.valor, 0) as valor,
                        isnull(R.plazo_tipo, '') as plazo_tipo,
                        isnull(R.plazo_dias, 0) as plazo_dias,
                        cast(isnull(R.Diferido_Cargo, 0) as bit) as diferido_cargo,
                        cast(1 as bit) as checked_ind
                    from cargos_adicionales R
                    inner join operacion_cargos A
                        on R.cod_cargo = A.cod_cargo
                       and A.id_solicitud = @Operacion
                    order by R.cod_cargo",
                    new { Operacion = operacion }).ToList());

            return response.Result ?? new List<CrSeguimientoReqCarCargoData>();
        }

        private static string NormalizarVentana(string? ventana)
        {
            string valor = (ventana ?? VentanaRequisitos).Trim().ToUpperInvariant();
            return valor == VentanaRequisitos ? VentanaRequisitos : VentanaCargos;
        }

        private static string NormalizarTipoConsulta(string? tipoConsulta)
        {
            string valor = (tipoConsulta ?? TipoConsultaManual).Trim().ToUpperInvariant();
            return valor == TipoConsultaRegistrado ? TipoConsultaRegistrado : TipoConsultaManual;
        }

        private static string NormalizarTipoCargo(string? tipo)
        {
            string valor = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (valor == "P" || valor.StartsWith("P"))
            {
                return "P";
            }

            return "M";
        }

        private static bool EsEditable(string? estadoSolicitud)
        {
            string estado = (estadoSolicitud ?? string.Empty).Trim().ToUpperInvariant();
            return estado != "N" && estado != "F";
        }
    }
}