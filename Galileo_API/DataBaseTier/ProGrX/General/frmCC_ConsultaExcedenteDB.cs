using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCcConsultaExcedenteDb
    {
        private const int ModuloGeneral = 10;
        private const int CodigoValidacion = -2;

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCcConsultaExcedenteDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los periodos de excedentes cerrados disponibles para consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Periodos cerrados ordenados del más reciente al más antiguo.</returns>
        public ErrorDto<List<CCPeriodoList>>
            CC_ConsultaExcedente_Periodos_Obtener(int codEmpresa)
        {
            const string sql = """
                select Idx, ItmX
                from vExc_Periodos
                where ESTADO = 'C'
                order by IdX desc
                """;

            return DbHelper.ExecuteListQuery<CCPeriodoList>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Valida el acceso al expediente y obtiene la persona indicada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Identificación y nombre de la persona autorizada.</returns>
        public ErrorDto<CcConsultaExcedentePersonaData>
            CC_ConsultaExcedente_Persona_Obtener(
                int codEmpresa,
                string cedula,
                string usuario)
        {
            string identificacion = cedula.Trim();
            string usuarioConsulta = usuario.Trim();

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la identificación de la persona.",
                    CodigoValidacion,
                    new CcConsultaExcedentePersonaData());
            }

            if (string.IsNullOrWhiteSpace(usuarioConsulta))
            {
                return DbHelper.CreateErrorResponse(
                    "No se pudo determinar el usuario de la consulta.",
                    CodigoValidacion,
                    new CcConsultaExcedentePersonaData());
            }

            var resultado = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var acceso = connection.QueryFirstOrDefault<ConsultaStatusResultDto>(
                        "spSYS_RA_Consulta_Status",
                        new
                        {
                            Cedula = identificacion,
                            Usuario = usuarioConsulta
                        },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 0);

                    if (acceso is null)
                    {
                        return DbHelper.CreateErrorResponse(
                            "No se encontró información de la persona.",
                            CodigoValidacion,
                            new CcConsultaExcedentePersonaData());
                    }

                    if (acceso.PERSONA_ID > 0 && acceso.AUTORIZACION_ID == 0)
                    {
                        return DbHelper.CreateErrorResponse(
                            "Esta persona tiene el expediente restringido. " +
                            "Requiere autorización para consultar.",
                            CodigoValidacion,
                            new CcConsultaExcedentePersonaData());
                    }

                    const string sql = """
                        select
                            rtrim(cedula) as cedula,
                            rtrim(isnull(nombre, '')) as nombre
                        from socios
                        where cedula = @identificacion
                        """;

                    var persona = connection
                        .QueryFirstOrDefault<CcConsultaExcedentePersonaData>(
                            sql,
                            new { identificacion });

                    return persona is null
                        ? DbHelper.CreateErrorResponse(
                            "No se encontró registro de la persona.",
                            CodigoValidacion,
                            new CcConsultaExcedentePersonaData())
                        : DbHelper.CreateOkResponse(persona);
                });

            if (resultado.Code != 0 || resultado.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ?? "No fue posible consultar la persona.",
                    resultado.Code ?? -1,
                    new CcConsultaExcedentePersonaData());
            }

            return resultado.Result;
        }

        /// <summary>
        /// Consulta el desglose del excedente y acumula todas las notas aplicables.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="idPeriodo">Identificador del periodo de excedentes.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Desglose del excedente, notas y fecha del servidor.</returns>
        public ErrorDto<CcConsultaExcedenteResultadoData>
            CC_ConsultaExcedente_Consultar(
                int codEmpresa,
                int idPeriodo,
                string cedula)
        {
            string identificacion = cedula.Trim();

            if (idPeriodo <= 0 || string.IsNullOrWhiteSpace(identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    "El periodo y la identificación son requeridos.",
                    CodigoValidacion,
                    new CcConsultaExcedenteResultadoData());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    const string sqlPeriodo = """
                        select NC_MORA, NC_OPCF, NC_SALDOS
                        from Exc_Periodos
                        where id_periodo = @idPeriodo
                        """;

                    var periodo = connection.QueryFirstOrDefault<CCExcPeriodoData>(
                        sqlPeriodo,
                        new { idPeriodo });

                    if (periodo is null)
                    {
                        throw new InvalidOperationException(
                            "El periodo indicado no existe.");
                    }

                    const string sqlExcedente = """
                        select
                            E.*,
                            isnull(S.DESCRIPCION, 'No Identificada') as SalidaDesc
                        from exc_cierre E
                        left join EXC_TIPOS_SALIDAS S
                            on E.SALIDA_CODIGO = S.COD_SALIDA
                        where E.id_periodo = @idPeriodo
                          and E.cedula = @identificacion
                        """;

                    var excedente = connection
                        .QueryFirstOrDefault<CCConsultaExcedenteData>(
                            sqlExcedente,
                            new { idPeriodo, identificacion });

                    var notas = new List<VSifAuxCreditosMovDetalle>();

                    if (excedente is not null)
                    {
                        AgregarNotasMora(
                            connection,
                            notas,
                            periodo.nc_mora,
                            identificacion,
                            excedente.mora_aplicada);

                        AgregarNotasOpcf(
                            connection,
                            notas,
                            periodo.nc_opcf,
                            identificacion,
                            excedente.moraopcf_aplicada);

                        AgregarNotasSaldos(
                            connection,
                            notas,
                            periodo.nc_saldos,
                            identificacion,
                            excedente.saldos_ase_aplicados);
                    }

                    return new CcConsultaExcedenteResultadoData
                    {
                        excedente = excedente,
                        notas = notas,
                        fecha_servidor = connection.QuerySingle<DateTime>(
                            "select dbo.Mygetdate()")
                    };
                });
        }

        /// <summary>
        /// Envía la boleta de excedentes a la bandeja de salida del asociado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Periodo, persona y usuario solicitante.</param>
        /// <returns>Resultado de la puesta en cola de la notificación.</returns>
        public ErrorDto CC_ConsultaExcedente_Email_Enviar(
            int codEmpresa,
            CcConsultaExcedenteEmailRequest request)
        {
            string cedula = request.cedula.Trim();
            string usuario = request.usuario.Trim();

            if (request.id_periodo <= 0 ||
                string.IsNullOrWhiteSpace(cedula) ||
                string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El periodo, la identificación y el usuario son requeridos.",
                    CodigoValidacion);
            }

            const string sql = """
                EXEC spExc_Notifica_Boleta
                    @idPeriodo,
                    @cedula,
                    @usuario
                """;

            var resultado = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    idPeriodo = request.id_periodo,
                    cedula,
                    usuario
                });

            if (resultado.Code != 0)
            {
                return resultado;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Notifica",
                $"Estado de Cuenta de Excedentes a: {cedula}, " +
                $"Periodo: {request.id_periodo}");

            return DbHelper.OkResponse(
                "Estado de excedentes enviado a la bandeja de salida.");
        }

        /// <summary>
        /// Registra la generación del reporte de excedentes en la bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Datos del periodo, usuario y alcance del reporte.</param>
        /// <returns>Resultado del registro de auditoría.</returns>
        public ErrorDto CC_ConsultaExcedente_Reporte_Bitacora_Registrar(
            int codEmpresa,
            CcConsultaExcedenteBitacoraRequest request)
        {
            string usuario = request.usuario.Trim();

            if (request.id_periodo <= 0 || string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El periodo y el usuario son requeridos.",
                    CodigoValidacion);
            }

            if (!request.todos && string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la identificación de la persona.",
                    CodigoValidacion);
            }

            string detalle = request.todos
                ? $"Estado Excedentes Todos. Periodo: {request.periodo.Trim()}"
                : $"Estado Excedentes Ced.{request.cedula.Trim()} " +
                  $"Periodo: {request.periodo.Trim()}";

            RegistrarBitacora(codEmpresa, usuario, "Imprime", detalle);
            return DbHelper.OkResponse("Movimiento registrado satisfactoriamente.");
        }

        private static void AgregarNotasMora(
            IDbConnection connection,
            List<VSifAuxCreditosMovDetalle> notas,
            string? numeroNota,
            string cedula,
            decimal montoAplicado)
        {
            if (string.IsNullOrWhiteSpace(numeroNota) || montoAplicado <= 0)
            {
                return;
            }

            const string sql = """
                select M.*
                from vSIFAuxCreditosMovDetalle M
                where M.tcon in ('7', 'NC')
                  and M.ncon = @numeroNota
                  and M.cedula = @cedula
                """;

            notas.AddRange(connection.Query<VSifAuxCreditosMovDetalle>(
                sql,
                new { numeroNota, cedula }));
        }

        private static void AgregarNotasOpcf(
            IDbConnection connection,
            List<VSifAuxCreditosMovDetalle> notas,
            string? numeroNota,
            string cedula,
            decimal montoAplicado)
        {
            if (string.IsNullOrWhiteSpace(numeroNota) || montoAplicado <= 0)
            {
                return;
            }

            const string sql = """
                select M.*
                from vSIFAuxCreditosMovDetalle M
                where M.tcon in ('7', 'NC')
                  and M.ncon = @numeroNota
                  and M.id_solicitud in (
                      select id_solicitud
                      from reg_creditos
                      where referencia in (
                          select id_solicitud
                          from reg_creditos
                          where cedula = @cedula
                            and garantia = 'F'
                      )
                  )
                """;

            var resultado = connection.Query<VSifAuxCreditosMovDetalle>(
                sql,
                new { numeroNota, cedula }).ToList();

            resultado.ForEach(item => item.concepto = $"OPCF: {item.concepto}");
            notas.AddRange(resultado);
        }

        private static void AgregarNotasSaldos(
            IDbConnection connection,
            List<VSifAuxCreditosMovDetalle> notas,
            string? numeroNota,
            string cedula,
            decimal montoAplicado)
        {
            if (string.IsNullOrWhiteSpace(numeroNota) || montoAplicado <= 0)
            {
                return;
            }

            const string sql = """
                select C.*
                from vSIFAuxCreditosMovDetalle C
                where C.tcon in ('7', 'NC')
                  and C.ncon = @numeroNota
                  and C.cedula = @cedula
                """;

            var resultado = connection.Query<VSifAuxCreditosMovDetalle>(
                sql,
                new { numeroNota, cedula }).ToList();

            resultado.ForEach(item => item.concepto = $"CEXD: {item.concepto}");
            notas.AddRange(resultado);
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario.Trim(),
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = ModuloGeneral
                });
        }

        #region Compatibilidad con el cliente anterior

        /// <summary>
        /// Obtiene los periodos con el contrato sin envoltorio del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Lista de periodos cerrados.</returns>
        public List<CCPeriodoList> CC_Periodos_Obtener(int codEmpresa) =>
            CC_ConsultaExcedente_Periodos_Obtener(codEmpresa).Result ?? [];

        /// <summary>
        /// Obtiene las notas configuradas para un periodo con el contrato anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="idPeriodo">Identificador del periodo.</param>
        /// <returns>Configuración de notas del periodo.</returns>
        public CCExcPeriodoData CC_Exc_Periodos_Obtener(
            int codEmpresa,
            int idPeriodo)
        {
            const string sql = """
                select NC_MORA, NC_OPCF, NC_SALDOS
                from Exc_Periodos
                where id_periodo = @idPeriodo
                """;

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                new CCExcPeriodoData(),
                new { idPeriodo }).Result ?? new CCExcPeriodoData();
        }

        /// <summary>
        /// Valida persona y acceso usando el contrato de códigos del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="usuario">Usuario que consulta.</param>
        /// <returns>Código uno para éxito y cero para rechazo.</returns>
        public ErrorDto CC_ValidaCedula_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            var resultado = CC_ConsultaExcedente_Persona_Obtener(
                codEmpresa,
                cedula,
                usuario);

            return new ErrorDto
            {
                Code = resultado.Code == 0 ? 1 : 0,
                Description = resultado.Code == 0
                    ? resultado.Result?.nombre ?? string.Empty
                    : resultado.Description
            };
        }

        /// <summary>
        /// Obtiene el desglose individual con el contrato del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="idPeriodo">Identificador del periodo.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Desglose del excedente o un objeto vacío.</returns>
        public CCConsultaExcedenteData CC_ConsultaExcedente_Obtener(
            int codEmpresa,
            int idPeriodo,
            string cedula) =>
            CC_ConsultaExcedente_Consultar(codEmpresa, idPeriodo, cedula)
                .Result?.excedente ?? new CCConsultaExcedenteData();

        /// <summary>
        /// Obtiene notas de mora con el contrato del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="numeroNota">Número de nota de crédito.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Movimientos correspondientes a la mora.</returns>
        public List<VSifAuxCreditosMovDetalle> CC_NotasMora_Obtener(
            int codEmpresa,
            int numeroNota,
            string cedula)
        {
            const string sql = """
                select M.*
                from vSIFAuxCreditosMovDetalle M
                where M.tcon in ('7', 'NC')
                  and M.ncon = @numeroNota
                  and M.cedula = @cedula
                """;

            return DbHelper.ExecuteListQuery<VSifAuxCreditosMovDetalle>(
                _portalDb,
                codEmpresa,
                sql,
                new { numeroNota, cedula }).Result ?? [];
        }

        /// <summary>
        /// Obtiene notas OPCF con el contrato del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="numeroNota">Número de nota de crédito.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Movimientos correspondientes a OPCF.</returns>
        public List<VSifAuxCreditosMovDetalle> CC_NotasOPCF_Obtener(
            int codEmpresa,
            int numeroNota,
            string cedula)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var notas = new List<VSifAuxCreditosMovDetalle>();
                    AgregarNotasOpcf(connection, notas, numeroNota.ToString(), cedula, 1);
                    return notas;
                }).Result ?? [];
        }

        /// <summary>
        /// Obtiene notas de saldos con el contrato del cliente anterior.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="numeroNota">Número de nota de crédito.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Movimientos correspondientes a créditos de excedentes.</returns>
        public List<VSifAuxCreditosMovDetalle> CC_NotasSaldos_Obtener(
            int codEmpresa,
            int numeroNota,
            string cedula)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var notas = new List<VSifAuxCreditosMovDetalle>();
                    AgregarNotasSaldos(connection, notas, numeroNota.ToString(), cedula, 1);
                    return notas;
                }).Result ?? [];
        }

        #endregion
    }
}
