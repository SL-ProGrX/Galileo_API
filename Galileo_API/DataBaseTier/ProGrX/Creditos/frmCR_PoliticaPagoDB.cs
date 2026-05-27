using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPoliticaPagoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;

        public FrmCrPoliticaPagoDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrPoliticaPagoDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene las politicas de pago configuradas en CRD_POLITICA_PAGO.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la politica.</param>
        /// <returns>Lista de politicas ordenadas por dia_inicio.</returns>
        public ErrorDto<List<CrPoliticaPagoData>> CR_PoliticaPago_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    id_politica,
                    dia_inicio,
                    dia_corte,
                    politica,
                    case Politica
                        when 'FOR' then 'Dia de la Formalizacion'
                        when 'ULT' then 'Ultimo dia del Mes'
                        when 'ESP' then 'Dia Especifico'
                    end as politica_desc,
                    dia_base
                from CRD_POLITICA_PAGO
                order by dia_inicio";

            return DbHelper.ExecuteListQuery<CrPoliticaPagoData>(_portalDb, codEmpresa, sqlQuery);
        }

        /// <summary>
        /// Guarda una politica de pago, insertando o actualizando segun id_politica.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se guarda CRD_POLITICA_PAGO.</param>
        /// <param name="usuario">Usuario que ejecuta el mantenimiento para bitacora.</param>
        /// <param name="request">Datos de politica: id_politica, dia_inicio, dia_corte, politica y dia_base.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_PoliticaPago_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoData request)
        {
            usuario = (usuario ?? string.Empty).Trim();
            NormalizarPolitica(request);

            if (!PoliticaPagoEsValida(request, out var mensaje))
                return DbHelper.ErrorResponse(mensaje);

            var idPolitica = request.id_politica.GetValueOrDefault();
            var existe = idPolitica > 0 && ExistePolitica(codEmpresa, idPolitica);
            if (!existe)
                request.id_politica = ObtenerSiguientePolitica(codEmpresa);

            var resp = existe
                ? ActualizarPolitica(codEmpresa, request)
                : InsertarPolitica(codEmpresa, request);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                existe ? "Modifica - WEB" : "Registra - WEB",
                $"Politica de Pago : {request.id_politica}");

            return DbHelper.OkResponse("Informacion guardada satisfactoriamente...");
        }

        /// <summary>
        /// Elimina una politica de pago por id_politica.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se elimina CRD_POLITICA_PAGO.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminacion para bitacora.</param>
        /// <param name="idPolitica">Identificador de politica a eliminar.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_PoliticaPago_Eliminar(
            int codEmpresa,
            string usuario,
            int idPolitica)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (idPolitica <= 0)
                return DbHelper.ErrorResponse("Debe seleccionar una politica valida.");

            const string sqlDelete = @"
                delete from CRD_POLITICA_PAGO
                where id_politica = @IdPolitica";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlDelete, new { IdPolitica = idPolitica });
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Politica de Pago : {idPolitica}");
            return DbHelper.OkResponse("Informacion eliminada satisfactoriamente...");
        }

        /// <summary>
        /// Obtiene los traslados de dias no habiles por tipo.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta CRD_POLITICA_PAGO_TRASLADOS.</param>
        /// <param name="tipo">Tipo de traslado: DS, DR o FE.</param>
        /// <returns>Lista de traslados del tipo indicado.</returns>
        public ErrorDto<List<CrPoliticaPagoTrasladoData>> CR_PoliticaPago_Traslados_Obtener(
            int codEmpresa,
            string tipo)
        {
            tipo = NormalizarTipoTraslado(tipo);

            const string sqlQuery = @"
                select
                    id_seq,
                    tipo,
                    case tipo
                        when 'DS' then 'Dia de la Semana'
                        when 'DR' then 'Dia Recurrente'
                        when 'FE' then 'Fecha Especifica'
                    end as tipo_desc,
                    dia_semana,
                    case dia_semana
                        when 1 then 'Domingo'
                        when 2 then 'Lunes'
                        when 3 then 'Martes'
                        when 4 then 'Miercoles'
                        when 5 then 'Jueves'
                        when 6 then 'Viernes'
                        when 7 then 'Sabado'
                    end as dia_semana_desc,
                    fecha_inicio,
                    fecha_corte,
                    case tipo
                        when 'DS' then case dia_semana
                            when 1 then 'Domingo'
                            when 2 then 'Lunes'
                            when 3 then 'Martes'
                            when 4 then 'Miercoles'
                            when 5 then 'Jueves'
                            when 6 then 'Viernes'
                            when 7 then 'Sabado'
                        end
                        when 'DR' then concat(day(fecha_inicio), ' de ', case month(fecha_inicio)
                            when 1 then 'Enero'
                            when 2 then 'Febrero'
                            when 3 then 'Marzo'
                            when 4 then 'Abril'
                            when 5 then 'Mayo'
                            when 6 then 'Junio'
                            when 7 then 'Julio'
                            when 8 then 'Agosto'
                            when 9 then 'Septiembre'
                            when 10 then 'Octubre'
                            when 11 then 'Noviembre'
                            when 12 then 'Diciembre'
                        end)
                        when 'FE' then concat(convert(varchar(10), fecha_inicio, 103), ' - ', convert(varchar(10), fecha_corte, 103))
                    end as descripcion
                from CRD_POLITICA_PAGO_TRASLADOS
                where tipo = @Tipo
                order by id_seq";

            return DbHelper.ExecuteListQuery<CrPoliticaPagoTrasladoData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new { Tipo = tipo });
        }

        /// <summary>
        /// Guarda un traslado de dias no habiles en CRD_POLITICA_PAGO_TRASLADOS.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se guarda el traslado.</param>
        /// <param name="usuario">Usuario que ejecuta el mantenimiento para bitacora.</param>
        /// <param name="request">Datos del traslado: tipo, dia_semana, fecha_inicio y fecha_corte.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_PoliticaPago_Traslados_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoTrasladoGuardarRequest request)
        {
            usuario = (usuario ?? string.Empty).Trim();
            request.tipo = NormalizarTipoTraslado(request.tipo);

            if (!TrasladoEsValido(request, out var mensaje))
                return DbHelper.ErrorResponse(mensaje);

            var idSeq = ObtenerSiguienteTraslado(codEmpresa);
            var resp = InsertarTraslado(codEmpresa, idSeq, request);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Politica de Pago [No Habil]: {idSeq}");
            return DbHelper.OkResponse("Politica de dias habiles registrada satisfactoriamente...!");
        }

        /// <summary>
        /// Elimina un traslado de dias no habiles por id_seq.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se elimina el traslado.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminacion para bitacora.</param>
        /// <param name="idSeq">Identificador de traslado a eliminar.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_PoliticaPago_Traslados_Eliminar(
            int codEmpresa,
            string usuario,
            int idSeq)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (idSeq <= 0)
                return DbHelper.ErrorResponse("Debe seleccionar una politica valida.");

            const string sqlDelete = @"
                delete from CRD_POLITICA_PAGO_TRASLADOS
                where id_seq = @IdSeq";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlDelete, new { IdSeq = idSeq });
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Politica de Pago [No Habil]: {idSeq}");
            return DbHelper.OkResponse("Politica de dias habiles eliminada satisfactoriamente...!");
        }

        /// <summary>
        /// Actualiza las tablas de pago con la fecha de pago habil.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se ejecuta spCrdPlanPagoDiaHabilActualiza.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso para bitacora.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto CR_PoliticaPago_TablasPago_Actualizar(int codEmpresa, string usuario)
        {
            usuario = (usuario ?? string.Empty).Trim();

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, "exec spCrdPlanPagoDiaHabilActualiza");
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Aplica - WEB", "Politica de Pago [No Habil]: Actualiza Tablas");
            return DbHelper.OkResponse("Tablas de Pago, Actualizadas [Fecha Pago]satisfactoriamente...!");
        }

        private static bool PoliticaPagoEsValida(CrPoliticaPagoData request, out string mensaje)
        {
            mensaje = string.Empty;

            if (request.dia_inicio < 1 || request.dia_inicio > 31 || request.dia_corte < 1 || request.dia_corte > 31)
                mensaje = "Los dias de inicio y corte deben estar entre 1 y 31.";

            if (request.politica == "ESP" && (request.dia_base < 1 || request.dia_base > 31))
                mensaje = "Debe indicar un dia base valido para la politica especifica.";

            return string.IsNullOrWhiteSpace(mensaje);
        }

        private static bool TrasladoEsValido(CrPoliticaPagoTrasladoGuardarRequest request, out string mensaje)
        {
            mensaje = string.Empty;

            if (request.tipo == "DS" && (request.dia_semana < 1 || request.dia_semana > 7))
                mensaje = "Debe indicar un dia de la semana valido.";

            if ((request.tipo == "DR" || request.tipo == "FE") && !request.fecha_inicio.HasValue)
                mensaje = "Debe indicar la fecha de inicio.";

            if (request.tipo == "FE" && !request.fecha_corte.HasValue)
                mensaje = "Debe indicar la fecha de corte.";

            if (request.tipo == "FE" && request.fecha_inicio > request.fecha_corte)
                mensaje = "La fecha de inicio no puede ser mayor que la fecha de corte.";

            return string.IsNullOrWhiteSpace(mensaje);
        }

        private bool ExistePolitica(int codEmpresa, int idPolitica)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from CRD_POLITICA_PAGO
                where id_politica = @IdPolitica";

            var resp = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExiste, 0, new { IdPolitica = idPolitica });
            return resp.Result > 0;
        }

        private int ObtenerSiguientePolitica(int codEmpresa)
        {
            const string sqlSeq = "select isnull(max(id_politica), 0) + 1 from CRD_POLITICA_PAGO";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlSeq, 1).Result;
        }

        private int ObtenerSiguienteTraslado(int codEmpresa)
        {
            const string sqlSeq = "select isnull(max(id_seq), 0) + 1 from CRD_POLITICA_PAGO_TRASLADOS";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlSeq, 1).Result;
        }

        private ErrorDto InsertarPolitica(int codEmpresa, CrPoliticaPagoData request)
        {
            const string sqlInsert = @"
                insert into CRD_POLITICA_PAGO(id_politica, dia_inicio, dia_corte, politica, dia_base)
                values(@IdPolitica, @DiaInicio, @DiaCorte, @Politica, @DiaBase)";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlInsert, ObtenerParametrosPolitica(request));
        }

        private ErrorDto ActualizarPolitica(int codEmpresa, CrPoliticaPagoData request)
        {
            const string sqlUpdate = @"
                update CRD_POLITICA_PAGO
                set dia_inicio = @DiaInicio,
                    dia_corte = @DiaCorte,
                    politica = @Politica,
                    dia_base = @DiaBase
                where id_politica = @IdPolitica";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlUpdate, ObtenerParametrosPolitica(request));
        }

        private ErrorDto InsertarTraslado(
            int codEmpresa,
            int idSeq,
            CrPoliticaPagoTrasladoGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into CRD_POLITICA_PAGO_TRASLADOS(id_seq, tipo, dia_semana, fecha_inicio, fecha_corte)
                values(@IdSeq, @Tipo, @DiaSemana, @FechaInicio, @FechaCorte)";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    IdSeq = idSeq,
                    Tipo = request.tipo,
                    DiaSemana = request.tipo == "DS" ? request.dia_semana : null,
                    FechaInicio = request.tipo == "DR" || request.tipo == "FE" ? request.fecha_inicio?.Date : null,
                    FechaCorte = request.tipo == "FE" ? request.fecha_corte?.Date : null
                });
        }

        private static object ObtenerParametrosPolitica(CrPoliticaPagoData request)
        {
            return new
            {
                IdPolitica = request.id_politica,
                DiaInicio = request.dia_inicio,
                DiaCorte = request.dia_corte,
                Politica = request.politica,
                DiaBase = request.dia_base
            };
        }

        private static void NormalizarPolitica(CrPoliticaPagoData request)
        {
            request.dia_inicio = request.dia_inicio <= 0 ? 1 : request.dia_inicio;
            request.dia_corte = request.dia_corte <= 0 ? 1 : request.dia_corte;
            request.politica = NormalizarCodigoPolitica(request.politica);
            request.dia_base = request.politica switch
            {
                "FOR" => 1,
                "ULT" => 32,
                _ => request.dia_base <= 0 ? 1 : request.dia_base
            };
        }

        private static string NormalizarCodigoPolitica(string politica)
        {
            politica = (politica ?? string.Empty).Trim().ToUpperInvariant();

            if (politica.StartsWith("FOR", StringComparison.Ordinal) || politica.StartsWith("DIA DE LA FORM", StringComparison.Ordinal))
                return "FOR";

            if (politica.StartsWith("ESP", StringComparison.Ordinal) || politica.StartsWith("DIA ESPEC", StringComparison.Ordinal))
                return "ESP";

            return "ULT";
        }

        private static string NormalizarTipoTraslado(string tipo)
        {
            tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipo.StartsWith("DIA DE LA SEMANA", StringComparison.Ordinal))
                return "DS";

            if (tipo.StartsWith("DIA RECURRENTE", StringComparison.Ordinal))
                return "DR";

            if (tipo.StartsWith("FECHA ESPEC", StringComparison.Ordinal))
                return "FE";

            return tipo is "DS" or "DR" or "FE" ? tipo : "DS";
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
