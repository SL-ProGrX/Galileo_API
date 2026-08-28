using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using System.Globalization;
using Dapper;

namespace Galileo.DataBaseTier
{
    public class FrmCcAutorizaSolicitudesDb
    {
        private const int CodigoErrorValidacion = -2;
        private const int TiempoEsperaSegundos = 300;
        private const string FormatoFecha = "yyyy-MM-dd";

        private readonly PortalDB _portalDb;

        public FrmCcAutorizaSolicitudesDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los bancos activos configurados para supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CCGenericList>>
            CC_Cuentas_Obtener(
                int CodEmpresa)
        {
            const string query = """
                select
                    convert(varchar(20), ID_BANCO) as idx,
                    rtrim(DESCRIPCION) as itmx
                from TES_BANCOS
                where ESTADO = 'A'
                  and supervision = 1
                order by DESCRIPCION;
                """;

            return DbHelper.ExecuteListQuery<CCGenericList>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las solicitudes de credito pendientes de supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBanco"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizaSolicitudesCreditoData>>
            CC_ModuloCredito_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            const string query = """
                select
                    R.id_solicitud,
                    R.codigo,
                    S.cedula,
                    S.nombre,
                    R.monto_girado
                from reg_creditos R
                inner join Socios S
                    on R.cedula = S.cedula
                inner join Catalogo C
                    on R.codigo = C.codigo
                   and C.retencion = 'N'
                   and C.poliza = 'N'
                where R.estadosol = 'F'
                  and R.fechaforp >= @FechaInicio
                  and R.fechaforp < @FechaCorteExclusiva
                  and R.tesoreria is null
                  and R.estado in ('A', 'C')
                  and R.id_solicitud not in
                  (
                      select id_solicitud
                      from CRD_REMESAS_TES_DETALLE
                  )
                  and dbo.fxTesSupervisa(
                      S.cedula,
                      S.nombre,
                      R.monto_girado,
                      0,
                      'C'
                  ) = 1
                  and R.TES_SUPERVISION_FECHA is null
                  and
                  (
                      @CodBanco is null
                      or R.cod_banco = @CodBanco
                  );
                """;

            return EjecutarConsultaConRango
                <AutorizaSolicitudesCreditoData>(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte,
                    query);
        }

        /// <summary>
        /// Obtiene las liquidaciones de fondos pendientes de supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBanco"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizaSolicitudesFondosData>>
            CC_ModuloFondos_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            const string query = """
                select
                    L.Consec,
                    C.Cedula,
                    S.nombre,
                    L.Cod_Plan,
                    L.Cod_Contrato,
                    case
                        when L.Total_Girar is null
                            then L.Aportes_Liq
                               + L.Rendi_Liq
                               - isnull(L.multa_retiro, 0)
                        else L.Total_Girar
                    end as Total_Girar
                from Fnd_Liquidacion L
                inner join Fnd_Contratos C
                    on L.Cod_Operadora = C.Cod_Operadora
                   and L.Cod_Plan = C.Cod_Plan
                   and L.Cod_Contrato = C.Cod_Contrato
                inner join Socios S
                    on C.cedula = S.cedula
                where L.Fecha >= @FechaInicio
                  and L.Fecha < @FechaCorteExclusiva
                  and L.Traspaso_tesoreria is null
                  and L.TES_SUPERVISION_FECHA is null
                  and dbo.fxTesSupervisa(
                      C.cedula,
                      S.nombre,
                      isnull(
                          L.Total_Girar,
                          L.Aportes_Liq
                          + L.Rendi_Liq
                          - isnull(L.multa_retiro, 0)
                      ),
                      0,
                      'C'
                  ) = 1
                  and
                  (
                      @CodBanco is null
                      or L.cod_banco = @CodBanco
                  );
                """;

            return EjecutarConsultaConRango
                <AutorizaSolicitudesFondosData>(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte,
                    query);
        }

        /// <summary>
        /// Obtiene las liquidaciones generales pendientes de supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBanco"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizaSolicitudesLiquidacionData>>
            CC_ModuloLiquidacion_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            const string query = """
                select
                    L.consec,
                    S.cedula,
                    S.nombre,
                    L.TNeto,
                    case
                        when L.EstadoActLiq = 'A'
                            then 'Ren.Asociación'
                        when L.EstadoActLiq = 'P'
                            then 'Ren.Patronal'
                    end as Tipo
                from Liquidacion L
                inner join Socios S
                    on L.cedula = S.cedula
                where L.FecLiq >= @FechaInicio
                  and L.FecLiq < @FechaCorteExclusiva
                  and L.Ubicacion = 'T'
                  and L.Estado = 'P'
                  and L.TES_SUPERVISION_FECHA is null
                  and dbo.fxTesSupervisa(
                      S.cedula,
                      S.nombre,
                      L.TNeto,
                      0,
                      'L'
                  ) = 1
                  and
                  (
                      @CodBanco is null
                      or L.cod_banco = @CodBanco
                  );
                """;

            return EjecutarConsultaConRango
                <AutorizaSolicitudesLiquidacionData>(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte,
                    query);
        }

        /// <summary>
        /// Obtiene los beneficios pendientes de supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBanco"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizaSolicitudesBeneficiosData>>
            CC_ModuloBeneficios_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            const string query = """
                select
                    B.Cedula,
                    B.consec,
                    B.cod_beneficio,
                    S.Nombre,
                    B.monto
                from afi_bene_pago B
                inner join socios S
                    on B.cedula = S.cedula
                inner join afi_bene_otorga O
                    on B.cod_beneficio = O.cod_beneficio
                   and B.consec = O.consec
                inner join Afi_Estados_Persona E
                    on S.EstadoActual = E.Cod_Estado
                inner join Tes_Bancos Ban
                    on B.cod_Banco = Ban.id_Banco
                where O.cod_remesa is null
                  and B.TES_SUPERVISION_FECHA is null
                  and O.registra_fecha >= @FechaInicio
                  and O.registra_fecha < @FechaCorteExclusiva
                  and B.ESTADO = 'S'
                  and B.tesoreria is null
                  and dbo.fxTesSupervisa(
                      B.cedula,
                      S.nombre,
                      B.monto,
                      0,
                      'C'
                  ) = 1
                  and
                  (
                      @CodBanco is null
                      or B.cod_banco = @CodBanco
                  );
                """;

            return EjecutarConsultaConRango
                <AutorizaSolicitudesBeneficiosData>(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte,
                    query);
        }

        /// <summary>
        /// Obtiene los desembolsos hipotecarios pendientes de supervision.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBanco"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="FechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizaSolicitudesHipotecarioData>>
            CC_ModuloHipotecario_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            const string query = """
                select
                    D.CodigoDesembolso,
                    D.NumeroOperacion,
                    D.Beneficiario,
                    D.Monto,
                    D.RegistroFecha,
                    D.RegistroUsuario,
                    S.cedula,
                    S.nombre,
                    R.codigo,
                    D.TES_SUPERVISION_FECHA
                from ViviendaDesembolsos D
                inner join Reg_Creditos R
                    on D.NumeroOperacion = R.id_solicitud
                inner join Socios S
                    on R.cedula = S.cedula
                where D.TesoreriaRemesa is null
                  and D.TES_SUPERVISION_FECHA is null
                  and D.RegistroFecha >= @FechaInicio
                  and D.RegistroFecha < @FechaCorteExclusiva
                  and dbo.fxTesSupervisa(
                      D.Identificacion,
                      D.Beneficiario,
                      D.Monto,
                      0,
                      'V'
                  ) = 1
                  and
                  (
                      @CodBanco is null
                      or R.cod_banco = @CodBanco
                  );
                """;

            return EjecutarConsultaConRango
                <AutorizaSolicitudesHipotecarioData>(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte,
                    query);
        }

        /// <summary>
        /// Autoriza una solicitud de credito para supervision de tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Id_Solicitud"></param>
        /// <returns></returns>
        public ErrorDto
            CC_ModuloCredito_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Id_Solicitud)
        {
            if (Id_Solicitud <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El identificador de la solicitud no es v&aacute;lido.",
                    CodigoErrorValidacion);
            }

            var errorUsuario = ObtenerErrorUsuario(Usuario);

            if (!string.IsNullOrEmpty(errorUsuario))
            {
                return DbHelper.ErrorResponse(
                    errorUsuario,
                    CodigoErrorValidacion);
            }

            const string query = """
                update REG_CREDITOS
                set TES_SUPERVISION_USUARIO = @Usuario,
                    TES_SUPERVISION_FECHA = getdate()
                where id_solicitud = @IdSolicitud;
                """;

            return EjecutarAutorizacion(
                CodEmpresa,
                query,
                new
                {
                    Usuario,
                    IdSolicitud = Id_Solicitud
                },
                $"Autorizaci&oacute;n de operaci&oacute;n {Id_Solicitud} procesada exitosamente.",
                $"No se encontr&oacute; la operaci&oacute;n {Id_Solicitud}.");
        }

        /// <summary>
        /// Autoriza una liquidacion de fondos para supervision de tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto
            CC_ModuloFondos_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            if (Consec <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El consecutivo de la liquidaci&oacute;n de fondos no es v&aacute;lido.",
                    CodigoErrorValidacion);
            }

            var errorUsuario = ObtenerErrorUsuario(Usuario);

            if (!string.IsNullOrEmpty(errorUsuario))
            {
                return DbHelper.ErrorResponse(
                    errorUsuario,
                    CodigoErrorValidacion);
            }

            const string query = """
                update Fnd_Liquidacion
                set TES_SUPERVISION_USUARIO = @Usuario,
                    TES_SUPERVISION_FECHA = getdate()
                where consec = @Consec;
                """;

            return EjecutarAutorizacion(
                CodEmpresa,
                query,
                new
                {
                    Usuario,
                    Consec
                },
                $"Autorizaci&oacute;n de Id {Consec} procesada exitosamente.",
                $"No se encontr&oacute; la liquidaci&oacute;n de fondos {Consec}.");
        }

        /// <summary>
        /// Autoriza una liquidaci&oacute;n general para supervision de tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto
            CC_ModuloLiquidacion_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            if (Consec <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El consecutivo de la liquidaci&oacute;n no es v&aacute;lido.",
                    CodigoErrorValidacion);
            }

            var errorUsuario = ObtenerErrorUsuario(Usuario);

            if (!string.IsNullOrEmpty(errorUsuario))
            {
                return DbHelper.ErrorResponse(
                    errorUsuario,
                    CodigoErrorValidacion);
            }

            const string query = """
                update Liquidacion
                set TES_SUPERVISION_USUARIO = @Usuario,
                    TES_SUPERVISION_FECHA = getdate()
                where consec = @Consec;
                """;

            return EjecutarAutorizacion(
                CodEmpresa,
                query,
                new
                {
                    Usuario,
                    Consec
                },
                $"Autorizaci&oacute;n de Id {Consec} procesada exitosamente.",
                $"No se encontr&oacute; la liquidaci&oacute;n {Consec}.");
        }

        /// <summary>
        /// Autoriza un beneficio para supervision de tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Consec"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <returns></returns>
        public ErrorDto
            CC_ModuloBeneficios_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec,
                string Cod_Beneficio)
        {
            if (Consec <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El consecutivo del beneficio no es v&aacute;lido.",
                    CodigoErrorValidacion);
            }

            var errorUsuario = ObtenerErrorUsuario(Usuario);

            if (!string.IsNullOrEmpty(errorUsuario))
            {
                return DbHelper.ErrorResponse(
                    errorUsuario,
                    CodigoErrorValidacion);
            }

            if (string.IsNullOrWhiteSpace(Cod_Beneficio))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del beneficio es requerido.",
                    CodigoErrorValidacion);
            }

            const string query = """
                update afi_bene_pago
                set TES_SUPERVISION_USUARIO = @Usuario,
                    TES_SUPERVISION_FECHA = getdate()
                where consec = @Consec
                  and cod_beneficio = @CodBeneficio;
                """;

            return EjecutarAutorizacion(
                CodEmpresa,
                query,
                new
                {
                    Usuario,
                    Consec,
                    CodBeneficio = Cod_Beneficio
                },
                $"Autorizaci&oacute;n de Id {Consec} procesada exitosamente.",
                $"No se encontr&oacute; el beneficio {Consec}.");
        }

        /// <summary>
        /// Autoriza un desembolso hipotecario para supervision de tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodigoDesembolso"></param>
        /// <returns></returns>
        public ErrorDto
            CC_ModuloHipotecario_Autorizar(
                int CodEmpresa,
                string Usuario,
                int CodigoDesembolso)
        {
            if (CodigoDesembolso <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del desembolso no es v&aacute;lido.",
                    CodigoErrorValidacion);
            }

            var errorUsuario = ObtenerErrorUsuario(Usuario);

            if (!string.IsNullOrEmpty(errorUsuario))
            {
                return DbHelper.ErrorResponse(
                    errorUsuario,
                    CodigoErrorValidacion);
            }

            const string query = """
                update ViviendaDesembolsos
                set TES_SUPERVISION_USUARIO = @Usuario,
                    TES_SUPERVISION_FECHA = getdate()
                where CodigoDesembolso = @CodigoDesembolso;
                """;

            return EjecutarAutorizacion(
                CodEmpresa,
                query,
                new
                {
                    Usuario,
                    CodigoDesembolso
                },
                $"Autorizaci&oacute;n de Id {CodigoDesembolso} procesada exitosamente.",
                $"No se encontr&oacute; el desembolso {CodigoDesembolso}.");
        }

        /// <summary>
        /// Ejecuta una consulta utilizando un rango de fechas validado. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="codBanco"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private ErrorDto<List<T>>
            EjecutarConsultaConRango<T>(
                int codEmpresa,
                int? codBanco,
                string fechaInicio,
                string fechaCorte,
                string query)
        {
            if (!TryObtenerRangoFechas(
                fechaInicio,
                fechaCorte,
                out var inicio,
                out var corteExclusivo,
                out var mensajeError))
            {
                return DbHelper.CreateErrorResponse<List<T>>(
                    mensajeError,
                    CodigoErrorValidacion,
                    []);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Query<T>(
                    query,
                    new
                    {
                        FechaInicio = inicio,
                        FechaCorteExclusiva = corteExclusivo,
                        CodBanco = codBanco
                    },
                    commandTimeout:
                        TiempoEsperaSegundos)
                    .ToList());
        }

        /// <summary>
        /// Ejecuta una autorizacion y valida los registros afectados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="query"></param>
        /// <param name="parametros"></param>
        /// <param name="mensajeExitoso"></param>
        /// <param name="mensajeSinRegistros"></param>
        /// <returns></returns>
        private ErrorDto
            EjecutarAutorizacion(
                int codEmpresa,
                string query,
                object parametros,
                string mensajeExitoso,
                string mensajeSinRegistros)
        {
            var respuesta = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                query,
                parametros);

            if (respuesta.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    respuesta.Description
                    ?? "No fue posible procesar la autorizaci&oacute;n.",
                    respuesta.Code ?? -1);
            }

            if (respuesta.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    mensajeSinRegistros,
                    CodigoErrorValidacion);
            }

            return DbHelper.OkResponse(
                mensajeExitoso);
        }

        /// <summary>
        /// Valida y convierte las fechas al rango requerido por SQL.
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="inicio"></param>
        /// <param name="corteExclusivo"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private static bool TryObtenerRangoFechas(
            string fechaInicio,
            string fechaCorte,
            out DateTime inicio,
            out DateTime corteExclusivo,
            out string mensajeError)
        {
            inicio = default;
            corteExclusivo = default;
            mensajeError = string.Empty;

            var fechaInicioValida = DateTime.TryParseExact(
                fechaInicio,
                FormatoFecha,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var inicioConvertido);

            var fechaCorteValida = DateTime.TryParseExact(
                fechaCorte,
                FormatoFecha,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var corteConvertido);

            if (!fechaInicioValida || !fechaCorteValida)
            {
                mensajeError =
                    $"Las fechas deben utilizar el formato {FormatoFecha}.";

                return false;
            }

            inicio = inicioConvertido.Date;
            var corte = corteConvertido.Date;

            if (inicio > corte)
            {
                mensajeError =
                    "La fecha inicial no puede ser mayor que la fecha final.";

                return false;
            }

            if (corte >= DateTime.MaxValue.Date)
            {
                mensajeError =
                    "La fecha final no es v&aacute;lida.";

                return false;
            }

            corteExclusivo = corte.AddDays(1);

            return true;
        }

        /// <summary>
        /// Valida el usuario requerido para procesar una autorizacion.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string ObtenerErrorUsuario(
            string usuario)
        {
            return string.IsNullOrWhiteSpace(usuario)
                ? "El usuario es requerido."
                : string.Empty;
        }
    }
}