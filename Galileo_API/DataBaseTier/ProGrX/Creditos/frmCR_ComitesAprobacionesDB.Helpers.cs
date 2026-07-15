using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrComitesAprobacionesDB
    {
        private static string QuerySolicitudes(CrComitesAprobacionesSolicitudRequest request)
        {
            var estado = EstadoSql("R.ESTADOSOL", request.estado);
            var linea = @"
                and (
                    isnull((select top 1 LINEA_FILTRA from COMITES where ID_COMITE = @id_comite),0) = 0
                    or R.CODIGO in (select CODIGO from CRD_COMITES_LINEAS where ID_COMITE = @id_comite)
                )";

            return $@"
                select
                    dbo.fxSemaforo(R.ID_SOLICITUD,R.ID_COMITE,'S') as semaforo,
                    cast(R.ID_SOLICITUD as varchar(30)) as expediente,
                    R.USERREC as usuario,
                    rtrim(isnull(R.CEDULA,'')) as cedula,
                    rtrim(isnull(S.NOMBRE,'')) as nombre,
                    rtrim(isnull(R.CODIGO,'')) as codigo,
                    isnull(R.MONTOSOL,0) as monto,
                    isnull(R.CUOTA,0) as cuota,
                    isnull(R.PLAZO,0) as plazo,
                    isnull(R.INT,0) as tasa,
                    case R.ESTADOSOL when 'R' then 'Recibido' when 'P' then 'Pendiente' else R.ESTADOSOL end as estado,
                    R.FECHASOL as fecha,
                    rtrim(isnull(R.GARANTIA,'')) as garantia,
                    rtrim(isnull(Gt.DESCRIPCION,'')) as garantia_desc
                from REG_CREDITOS R
                inner join SOCIOS S on S.CEDULA = R.CEDULA
                inner join CRD_COMITES_RNG_GARANTIA G on G.COD_GARANTIA = R.GARANTIA and G.ID_COMITE = R.ID_COMITE
                inner join CRD_GARANTIA_TIPOS Gt on R.GARANTIA = Gt.GARANTIA
                where R.ID_COMITE = @id_comite
                  and R.MONTOSOL between G.RNG_INICIO and G.RNG_CORTE
                  and R.FECHASOL between @FechaInicio and @FechaCorte
                  {estado}
                  and dbo.fxCRDTagAprobacion(R.ID_SOLICITUD) = 0
                  {linea}
                order by R.FECHASOL;";
        }

        private static string QueryEstudios(CrComitesAprobacionesSolicitudRequest request)
        {
            var estado = EstadoSql("P.ESTADO", request.estado);
            return $@"
                select
                    dbo.fxSemaforo(P.COD_PREANALISIS,P.ID_COMITE,'P') as semaforo,
                    cast(P.COD_PREANALISIS as varchar(30)) as expediente,
                    P.USUARIO as usuario,
                    rtrim(isnull(P.CEDULA,'')) as cedula,
                    rtrim(isnull(S.NOMBRE,'')) as nombre,
                    rtrim(isnull(P.COD_LINEA,'')) as codigo,
                    isnull(P.MONTO,0) as monto,
                    isnull(P.CUOTA,0) as cuota,
                    isnull(P.PLAZO,0) as plazo,
                    isnull(P.TASA,0) as tasa,
                    case P.ESTADO when 'R' then 'Recibido' when 'P' then 'Pendiente' else P.ESTADO end as estado,
                    P.FECHA_CREACION as fecha,
                    rtrim(isnull(P.GARANTIA,'')) as garantia,
                    rtrim(isnull(Gt.DESCRIPCION,'')) as garantia_desc
                from CRD_PREA_PREANALISIS P
                inner join SOCIOS S on S.CEDULA = P.CEDULA
                inner join CRD_COMITES_RNG_GARANTIA G on G.COD_GARANTIA = P.GARANTIA and G.ID_COMITE = P.ID_COMITE
                inner join CRD_GARANTIA_TIPOS Gt on P.GARANTIA = Gt.GARANTIA
                where P.TIPO_PREANALISIS = 'E'
                  and P.ID_COMITE = @id_comite
                  and P.MONTO between G.RNG_INICIO and G.RNG_CORTE
                  and P.FECHA_CREACION between @FechaInicio and @FechaCorte
                  {estado}
                  and (
                    isnull((select top 1 LINEA_FILTRA from COMITES where ID_COMITE = @id_comite),0) = 0
                    or P.COD_LINEA in (select CODIGO from CRD_COMITES_LINEAS where ID_COMITE = @id_comite)
                  )
                order by P.FECHA_CREACION;";
        }

        private static string EstadoSql(string campo, string estado)
        {
            return estado switch
            {
                "Recibida" => $"and {campo} = 'R'",
                "Pendiente" => $"and {campo} = 'P'",
                _ => $"and {campo} in ('P','R')"
            };
        }

        private static ErrorDto ValidarFiltrosSolicitud(CrComitesAprobacionesSolicitudRequest request)
        {
            if (request == null || request.id_comite <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un comite valido.", -2);
            }

            if (!EsSolicitud(request.tipo_caso) && !request.tipo_caso.Trim().Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("Debe indicar un tipo de caso valido.", -2);
            }

            return DbHelper.OkResponse(string.Empty);
        }

        private static void CR_ComitesAprobaciones_Deudas_CargarResumen(
            IDbConnection conn,
            string cedula,
            CrComitesAprobacionesDeudasResponse response)
        {
            const string sqlResumen = @"
                select
                    isnull(sum(R.SALDO), 0) as total_saldo,
                    isnull(sum(R.CUOTA), 0) as total_cuota
                from REG_CREDITOS R
                where R.SALDO > 0
                  and R.ESTADO = 'A'
                  and R.CEDULA = @cedula;";

            var resumen = conn.QueryFirstOrDefault(sqlResumen, new { cedula });
            if (resumen == null)
            {
                return;
            }

            response.total_saldo = resumen.total_saldo ?? 0;
            response.total_cuota = resumen.total_cuota ?? 0;
        }

        private static void CR_ComitesAprobaciones_Deudas_CargarDeducciones(
            IDbConnection conn,
            string tipoCaso,
            string operacion,
            CrComitesAprobacionesDeudasResponse response)
        {
            var codPreanalisis = ObtenerCodPreanalisis(conn, tipoCaso, operacion);
            if (string.IsNullOrWhiteSpace(codPreanalisis) || codPreanalisis == "0")
            {
                return;
            }

            const string sqlDeducciones = @"
                select isnull(sum(CUOTA_MENSUAL), 0)
                from CRD_PREA_DETALLE_DEDUC
                where COD_PREANALISIS = @cod_preanalisis;";

            response.deducciones = conn.QueryFirstOrDefault<decimal>(
                sqlDeducciones,
                new { cod_preanalisis = codPreanalisis });
        }

        private static string ObtenerCodPreanalisis(IDbConnection conn, string tipoCaso, string operacion)
        {
            if (!EsSolicitud(tipoCaso))
            {
                return operacion?.Trim() ?? string.Empty;
            }

            const string sql = @"
                select isnull(COD_PREANALISIS, 0)
                from CRD_PREA_PREANALISIS
                where TIPO_PREANALISIS = 'E'
                  and ID_SOLICITUD = @id_solicitud;";

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { id_solicitud = operacion?.Trim() ?? string.Empty }) ?? string.Empty;
        }

        private static string ObtenerIdSolicitudCaso(IDbConnection conn, string tipoCaso, string operacion)
        {
            var operacionNormalizada = operacion?.Trim() ?? string.Empty;
            if (EsSolicitud(tipoCaso))
            {
                return operacionNormalizada;
            }

            const string sql = @"
                select top 1 isnull(ID_SOLICITUD, 0)
                from CRD_PREA_PREANALISIS
                where TIPO_PREANALISIS = 'E'
                  and (
                    cast(COD_PREANALISIS as varchar(50)) = @operacion
                    or cast(COD_PREANALISIS_REF as varchar(50)) = @operacion
                  );";

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { operacion = operacionNormalizada }) ?? string.Empty;
        }

        private static List<CrComitesAprobacionesDeuda> CR_ComitesAprobaciones_Deudas_CargarLista(IDbConnection conn, string cedula)
        {
            const string sqlLista = "exec spSIFEstadoCreditos @cedula";
            return conn.Query(sqlLista, new { cedula })
                .Select(CR_ComitesAprobaciones_Deudas_MapearRow)
                .ToList();
        }

        private static CrComitesAprobacionesDeuda CR_ComitesAprobaciones_Deudas_MapearRow(dynamic row)
        {
            var datos = (IDictionary<string, object>)row;
            return new CrComitesAprobacionesDeuda
            {
                semaforo = CR_ComitesAprobaciones_Deudas_ResolverSemaforo(datos),
                operacion = Texto(datos, "id_solicitud"),
                linea = Texto(datos, "codigo"),
                plazo = Decimal(datos, "plazo"),
                monto = Decimal(datos, "MontoApr"),
                saldo = Decimal(datos, "Saldo"),
                cuota = Decimal(datos, "Cuota"),
                monto_atrasado = Decimal(datos, "MoraPrincipal") + Decimal(datos, "MoraInt"),
                primer_deduc = CR_ComitesAprobaciones_Deudas_FormatearPrimerMovimiento(datos),
                ultimo_movimiento = Texto(datos, "UltMovimien"),
                termina = Texto(datos, "Termina"),
                garantia = Texto(datos, "Garantia"),
                estado = Texto(datos, "Estado"),
                proceso = Texto(datos, "ProcesoCod"),
                operacion_referencia = Texto(datos, "Referencia"),
                tasa_original = Decimal(datos, "TasaOriginal"),
                tasa_actual = Decimal(datos, "Tasa"),
            };
        }

        private static string CR_ComitesAprobaciones_Deudas_ResolverSemaforo(IDictionary<string, object> datos)
        {
            var moraCuota = Decimal(datos, "MoraCuota");
            var procesoCod = Texto(datos, "ProcesoCod");
            var estado = Texto(datos, "Estado");
            var referencia = Texto(datos, "Referencia");
            var indicadorCbr = Decimal(datos, "IndicadorCbr");

            if (moraCuota > 0 && procesoCod != "J")
            {
                return "rojo";
            }

            if (procesoCod == "J")
            {
                return "judicial";
            }

            if (!string.IsNullOrWhiteSpace(referencia) && moraCuota == 0)
            {
                return "amarillo";
            }

            if (indicadorCbr > 0)
            {
                return "reversado";
            }

            if (estado.StartsWith('C'))
            {
                return "cancelado";
            }

            return "verde";
        }

        private static string CR_ComitesAprobaciones_Deudas_FormatearPrimerMovimiento(IDictionary<string, object> datos)
        {
            var primerDeduc = Decimal(datos, "prideduc");
            return primerDeduc <= 0 ? string.Empty : primerDeduc.ToString("0000-00");
        }

        private static ErrorDto ValidarResolucion(CrComitesAprobacionesResolucionRequest request)
        {
            if (request == null || request.id_comite <= 0 || string.IsNullOrWhiteSpace(request.acta) || string.IsNullOrWhiteSpace(request.operacion))
            {
                return DbHelper.ErrorResponse("Debe indicar comite, acta y caso.", -2);
            }

            if (!request.usuarios.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                return DbHelper.ErrorResponse("Debe indicar al menos un usuario autorizador.", -2);
            }

            return DbHelper.OkResponse(string.Empty);
        }

        private static bool EsSolicitud(string tipoCaso)
        {
            return tipoCaso.Trim().Equals("S", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarEstado(string estado, out string estadoComite, out int editable)
        {
            editable = 0;
            estadoComite = "APRO";

            switch (estado.Trim().ToUpperInvariant())
            {
                case "P":
                    estadoComite = "PEND";
                    editable = 1;
                    return "P";
                case "D":
                    estadoComite = "DESC";
                    return "D";
                case "V":
                    estadoComite = "PENVB";
                    editable = 1;
                    return "P";
                case "VL":
                    estadoComite = "PNVBL";
                    editable = 1;
                    return "P";
                default:
                    return "A";
            }
        }

        private static string PrimerUsuario(CrComitesAprobacionesResolucionRequest request)
        {
            return UsuarioEnIndice(request, 0);
        }

        private static string UsuarioEnIndice(CrComitesAprobacionesResolucionRequest request, int index)
        {
            return request.usuarios.Count > index ? request.usuarios[index].Trim() : string.Empty;
        }

        private static string Truncar(string valor, int max)
        {
            var texto = valor?.Trim() ?? string.Empty;
            return texto.Length <= max ? texto : texto[..max];
        }

        private static CrComitesAprobacionesDetalle MapDetalle(dynamic? row)
        {
            if (row == null)
            {
                return new CrComitesAprobacionesDetalle();
            }

            var datos = (IDictionary<string, object>)row;
            return new CrComitesAprobacionesDetalle
            {
                caso_id = Texto(datos, "Caso_Id"),
                cedula = Texto(datos, "Cedula"),
                nombre = Texto(datos, "Nombre"),
                membresia = Texto(datos, "Membresia"),
                codigo = Texto(datos, "Codigo"),
                estado_laboral_desc = Texto(datos, "EstadoLaboral_Desc"),
                estado_persona_desc = Texto(datos, "EstadoPersona_Desc"),
                monto = Decimal(datos, "Monto"),
                cuota = Decimal(datos, "Cuota"),
                monto_girado = Decimal(datos, "monto_girado"),
                desembolso_monto = Decimal(datos, "Desembolso_Monto"),
                desembolso_cuota = Decimal(datos, "DESEMBOLSO_CUOTA"),
                refunde_monto = Decimal(datos, "REFUNDE_MONTO"),
                refunde_cuota = Decimal(datos, "REFUNDE_CUOTA"),
                lugar_trabajo = Texto(datos, "LUGAR_TRABAJO"),
                ca = Decimal(datos, "CA"),
                cod_categoria_asociado = Texto(datos, "COD_CATEGORIA_ASOCIADO")
            };
        }

        private static string Texto(IDictionary<string, object> datos, string campo)
        {
            return TryGetCampo(datos, campo, out var valor) ? Convert.ToString(valor)?.Trim() ?? string.Empty : string.Empty;
        }

        private static decimal Decimal(IDictionary<string, object> datos, string campo)
        {
            if (!TryGetCampo(datos, campo, out var valor) || valor == null || valor == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(valor);
        }

        private static bool TryGetCampo(IDictionary<string, object> datos, string campo, out object? valor)
        {
            if (datos.TryGetValue(campo, out var directo))
            {
                valor = directo;
                return true;
            }

            var llave = datos.Keys.FirstOrDefault(x => x.Equals(campo, StringComparison.OrdinalIgnoreCase));
            if (llave != null)
            {
                valor = datos[llave];
                return true;
            }

            valor = null;
            return false;
        }
    }
}
