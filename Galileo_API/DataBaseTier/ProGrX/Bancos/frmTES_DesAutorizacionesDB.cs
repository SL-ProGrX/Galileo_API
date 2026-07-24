using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.TES;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesDesAutorizacionesDB
    {
        private const int MaxSolicitudesPorPeticion = 50000;
        private readonly PortalDB _portalDB;

        public FrmTesDesAutorizacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener solicitudes autorizadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesSolicitudesLista> TES_DesAutorizaciones_Obtener(int CodEmpresa, string filtros)
        {
            TesDesAutorizacionesFiltros filtro = JsonConvert.DeserializeObject<TesDesAutorizacionesFiltros>(filtros) ?? new TesDesAutorizacionesFiltros();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesSolicitudesLista>
            {
                Code = 0,
                Description = "",
                Result = new TesSolicitudesLista(),
            };
            var fechaInicio = filtro.fecha_inicio.Date;
            var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                var Rangos = conn.Query<TesAutorizacionData>(FrmTesAutorizacionSql.SQL_TES_AUTORIZACIONES_RANGOS,
                    new { filtro.usuario }).FirstOrDefault();

                if (Rangos != null)
                {
                    filtro.monto_inicio = Rangos.rango_gen_inicio ?? 0;
                    filtro.monto_fin = Rangos.rango_gen_corte ?? 0;
                }

                var query = $@"select T.nsolicitud,T.codigo,T.beneficiario,T.monto,T.fecha_solicitud,T.cta_Ahorros
                    ,0 as 'duplicado', dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) as 'Cta_Verifica'
                    , T.Detalle1 + T.detalle2 as 'Detalle', isnull(T.cod_App,'') as 'AppId'
                    from Tes_Transacciones T inner join Tes_Bancos B on T.id_banco = B.id_banco
                    where T.estado = 'P' and B.id_banco = @Banco and T.Tipo = @TipoDoc";

                if (!filtro.todas_fechas)
                {
                    query += " and T.fecha_solicitud between @FechaInicio and @FechaFin ";
                }

                if (!filtro.todas_solicitudes)
                {
                    query += " and (T.nsolicitud >= @SolicitudInicio and nsolicitud <= @SolicitudCorte ) ";
                }

                if (!filtro.casos_bloqueados)
                {
                    query += " and T.fecha_hold is null";
                }

                if (filtro.tipo_autorizacion == 0)
                {
                    query += " and T.fecha_autorizacion is not null and T.monto between @MontoInicio and @MontoFin ";
                    
                }
                else
                {
                    query += @" and T.FIRMAS_AUTORIZA_FECHA is not null and T.monto > B.firmas_hasta";
                }

                if (!string.IsNullOrWhiteSpace(filtro.token))
                {
                    query += " and T.id_token = @Token ";
                }
                    

                if (!string.IsNullOrWhiteSpace(filtro.detalle))
                {
                    query += " and (T.DETALLE1 + T.DETALLE2) like @Detalle ";
                }

                if (!string.IsNullOrWhiteSpace(filtro.appid))
                {
                    query += " and isnull(T.COD_APP,'') like @CodigoApp ";
                }

                response.Result.solicitudes = conn.Query<TesSolicitudesData>(query,
                    new
                    {
                        Banco = filtro.id_banco,
                        TipoDoc = filtro.tipo_doc,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaCorte,
                        SolicitudInicio = filtro.solicitud_inicio,
                        SolicitudCorte = filtro.solicitud_corte,
                        MontoInicio = filtro.monto_inicio,
                        MontoFin = filtro.monto_fin,
                        Detalle = $"%{filtro.detalle}%",
                        CodigoApp = $"%{filtro.appid}%",
                        Token = filtro.token
                    }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesSolicitudesLista>(ex.Message);
            }
            return response;
        }

        // <summary>
        /// Aplica la desautorización de las solicitudes seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="clave">Clave del autorizador.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="tipo_autorizacion">
        /// Tipo de autorización: 0 para emisión y 1 para firma electrónica.
        /// </param>
        /// <param name="solicitudesLista">Solicitudes serializadas en JSON.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto TES_DesAutorizaciones_Aplicar(
            int CodEmpresa,
            string clave,
            string usuario,
            int tipo_autorizacion,
            List<int> solicitudesLista)
        {
            
            if (solicitudesLista is not { Count: > 0 })
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos una solicitud para desautorizar.");
            }

            if (solicitudesLista.Count > MaxSolicitudesPorPeticion)
            {
                return DbHelper.ErrorResponse(
                    $"No se pueden desautorizar más de {MaxSolicitudesPorPeticion} solicitudes por petición.");
            }

            if (tipo_autorizacion is not 0 and not 1)
            {
                return DbHelper.ErrorResponse(
                    "El tipo de autorización no es válido.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var autorizacion = conn.QueryFirstOrDefault<TesAutorizacionData>(
                    FrmTesAutorizacionSql.Query_Autorizaciones,
                    new
                    {
                        clave,
                        usuario
                    });

                if (autorizacion == null)
                {
                    return DbHelper.ErrorResponse(
                        "Contraseña incorrecta o no existe nivel de autorización.");
                }

                string estado = tipo_autorizacion == 0 ? "D" : "X";

                FrmTesAutorizacionesLotesDB.TES_Autorizaciones_InsertarSolicitudes(
                    conn,
                    solicitudesLista,
                    estado,
                    usuario);

                conn.Execute(
                    "EXEC spTes_Mass_Aplica @Usuario, @Estado",
                    new
                    {
                        Usuario = usuario,
                        Estado = estado
                    },
                    commandTimeout:0);

                return DbHelper.OkResponse(
                    "Desautorización procesada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}


