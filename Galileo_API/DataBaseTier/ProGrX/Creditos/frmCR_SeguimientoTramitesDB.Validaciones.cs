using System.Data;
using Dapper;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private static CrSeguimientoTramitesRecepcionValidacion
            Cr_SeguimientoTramites_Recepcion_Validar(
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionGuardarRequest request,
                DateTime fechaSistema)
        {
            var result = new CrSeguimientoTramitesRecepcionValidacion
            {
                mensajes = Cr_SeguimientoTramites_Recepcion_ValidacionesBasicas_Obtener(
                    request,
                    fechaSistema)
            };

            CrSeguimientoTramitesRecepcionCatalogoRaw? catalogo =
                Cr_SeguimientoTramites_Recepcion_Catalogo_Validar(conn, request, result.mensajes);

            if (catalogo is not null)
            {
                result.base_calculo = catalogo.base_calculo.Trim();
                Cr_SeguimientoTramites_Recepcion_CobroJudicial_Validar(
                    conn,
                    request,
                    catalogo.permite_cbr,
                    result.mensajes);
            }

            Cr_SeguimientoTramites_Recepcion_Referencias_Validar(conn, request, result.mensajes);
            Cr_SeguimientoTramites_Recepcion_Rangos_Validar(conn, request, result.mensajes);
            Cr_SeguimientoTramites_Recepcion_Fondo_Validar(conn, request, result.mensajes);
            return result;
        }

        private static List<string> Cr_SeguimientoTramites_Recepcion_ValidacionesBasicas_Obtener(
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            DateTime fechaSistema)
        {
            var mensajes = new List<string>();

            if (string.Equals(request.emite_tipo?.Trim(), "CP", StringComparison.OrdinalIgnoreCase)
                && request.proveedor_id.GetValueOrDefault() <= 0)
            {
                mensajes.Add("- No se ha indicado a ningún Proveedor para la Cuenta por Pagar");
            }

            if (request.fecha_vence.HasValue
                && request.fecha_vence.Value.Date <= fechaSistema.Date)
            {
                mensajes.Add("La fecha de Vencimiento no puede ser igual o menor a la actual");
            }

            if (request.operacion == 0
                || string.Equals(
                    request.estado_solicitud?.Trim(),
                    "R",
                    StringComparison.OrdinalIgnoreCase))
            {
                Cr_SeguimientoTramites_Recepcion_ValoresSolicitados_Validar(request, mensajes);
            }

            return mensajes;
        }

        private static void Cr_SeguimientoTramites_Recepcion_ValoresSolicitados_Validar(
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            ICollection<string> mensajes)
        {
            if (request.plazo < 1)
            {
                mensajes.Add("- El Plazo Solicitado NO es válido");
            }

            if (request.tasa < 0)
            {
                mensajes.Add("- La Tasa solicitada no es válida");
            }

            if (request.monto < 1)
            {
                mensajes.Add("- El Monto Solicitado NO es válido");
            }
        }

        private static CrSeguimientoTramitesRecepcionCatalogoRaw?
            Cr_SeguimientoTramites_Recepcion_Catalogo_Validar(
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionGuardarRequest request,
                ICollection<string> mensajes)
        {
            const string sql = """
                select
                    ctaNintC as cta_nint_c,
                    isnull(retencion, '') as retencion,
                    isnull(poliza, '') as poliza,
                    isnull(activo, 0) as activo,
                    isnull(Permite_PersonaEnCbrJud, 0) as permite_cbr,
                    rtrim(isnull(base_calculo, '')) as base_calculo
                from catalogo
                where codigo = @Codigo;
                """;

            CrSeguimientoTramitesRecepcionCatalogoRaw? catalogo =
                conn.QueryFirstOrDefault<CrSeguimientoTramitesRecepcionCatalogoRaw>(
                    sql,
                    new { Codigo = request.codigo.Trim() });

            if (catalogo is null)
            {
                mensajes.Add("- El código de préstamo no existe");
                return null;
            }

            if (catalogo.cta_nint_c is null)
            {
                mensajes.Add("- El código no se encuentra codificado contablemente");
            }

            if (string.Equals(catalogo.retencion.Trim(), "S", StringComparison.OrdinalIgnoreCase)
                || string.Equals(catalogo.poliza.Trim(), "S", StringComparison.OrdinalIgnoreCase))
            {
                mensajes.Add("- No se permite guardar porque el código pertenece a una Retención o Póliza");
            }

            if (catalogo.activo == 0)
            {
                mensajes.Add("- La Línea de Crédito no se encuentra Activa...");
            }

            return catalogo;
        }

        private static void Cr_SeguimientoTramites_Recepcion_Referencias_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            ICollection<string> mensajes)
        {
            const string sql = """
                select count(1)
                from tes_banco_asg
                where id_banco = @BancoId and nombre = @Usuario;

                select count(1)
                from CRD_CATALOGO_ESTADOS E
                inner join Socios S on E.cod_Estado = S.EstadoActual
                where S.cedula = @Cedula and E.codigo = @Codigo;

                select count(1)
                from catalogo_destinosASG
                where codigo = @Codigo and cod_destino = @Destino;

                select count(1) from tes_bancos where id_banco = @BancoId;
                select count(1) from comites where id_comite = @ComiteId;
                select count(1)
                from crd_catalogo_garantias
                where codigo = @Codigo and garantia = @Garantia;
                """;

            var parameters = new
            {
                BancoId = request.banco_id,
                Usuario = request.usuario.Trim(),
                Cedula = request.cedula.Trim(),
                Codigo = request.codigo.Trim(),
                Destino = request.destino.Trim(),
                ComiteId = request.comite_id,
                Garantia = request.garantia.Trim()
            };

            using SqlMapper.GridReader grid = conn.QueryMultiple(sql, parameters);
            int bancoAsignado = grid.ReadSingle<int>();
            int estadoPersona = grid.ReadSingle<int>();
            int destino = grid.ReadSingle<int>();
            int banco = grid.ReadSingle<int>();
            int comite = grid.ReadSingle<int>();
            int garantia = grid.ReadSingle<int>();

            Cr_SeguimientoTramites_Recepcion_Referencias_Mensajes_Agregar(
                request,
                mensajes,
                bancoAsignado,
                estadoPersona,
                destino,
                banco,
                comite,
                garantia);
        }

        private static void Cr_SeguimientoTramites_Recepcion_Referencias_Mensajes_Agregar(
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            ICollection<string> mensajes,
            int bancoAsignado,
            int estadoPersona,
            int destino,
            int banco,
            int comite,
            int garantia)
        {
            if ((string.Equals(request.estado_solicitud.Trim(), "P", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.estado_solicitud.Trim(), "R", StringComparison.OrdinalIgnoreCase))
                && bancoAsignado == 0)
            {
                mensajes.Add(
                    $"- EL BANCO INDICADO NO SE ENCUENTRA AUTORIZADO AL USUARIO : {request.usuario.Trim()}");
            }

            if (estadoPersona == 0)
            {
                mensajes.Add(
                    "- Esta Línea de Crédito no Admite El estado actual de la persona (verifique.!)");
            }

            if (destino == 0)
            {
                mensajes.Add("- El Destino No es válido para Esta Línea");
            }

            if (banco == 0)
            {
                mensajes.Add("- El Banco Especificado NO EXISTE");
            }

            if (comite == 0)
            {
                mensajes.Add("- El Comité Especificado NO EXISTE");
            }

            if (!Cr_SeguimientoTramites_Recepcion_Estado_EsValido(request.estado_solicitud))
            {
                mensajes.Add("- El Estado de la Operación NO ES VALIDO");
            }

            if (!Cr_SeguimientoTramites_Recepcion_Emision_EsValida(request.emite_tipo))
            {
                mensajes.Add("- La emisión de la operación NO ES VALIDA");
            }

            if (garantia == 0)
            {
                mensajes.Add("- La Garantía especificada NO ES VALIDA");
            }
        }

        private static bool Cr_SeguimientoTramites_Recepcion_Estado_EsValido(string estado)
        {
            return estado.Trim().ToUpperInvariant() is "R" or "P" or "A" or "D" or "N" or "F";
        }

        private static bool Cr_SeguimientoTramites_Recepcion_Emision_EsValida(string emision)
        {
            return emision.Trim().ToUpperInvariant() is "CK" or "TE" or "TS" or "ND" or "CD"
                or "CP" or "RC";
        }

        private static void Cr_SeguimientoTramites_Recepcion_Rangos_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            ICollection<string> mensajes)
        {
            if (mensajes.Count > 0
                || (request.operacion != 0
                    && !string.Equals(
                        request.estado_solicitud.Trim(),
                        "R",
                        StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            string? mensaje = conn.QueryFirstOrDefault<string>(
                """
                exec spCrdFormaliza_Valida_Rangos
                    @Cedula, @Codigo, @Monto, @Tasa, @Plazo, @Destino, @Garantia, @Operacion;
                """,
                new
                {
                    Cedula = request.cedula.Trim(),
                    Codigo = request.codigo.Trim(),
                    Monto = request.monto,
                    Tasa = request.tasa,
                    Plazo = request.plazo,
                    Destino = request.destino.Trim(),
                    Garantia = request.garantia.Trim(),
                    Operacion = request.operacion
                });

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                mensajes.Add(mensaje.Trim());
            }
        }

        private static void Cr_SeguimientoTramites_Recepcion_Fondo_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            ICollection<string> mensajes)
        {
            if (!string.Equals(request.garantia.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.fnd_garantia))
            {
                mensajes.Add(" - No existe un PLAN especificado para cobertura de esta garantía");
                return;
            }

            decimal disponible = conn.QueryFirstOrDefault<decimal>(
                "exec spCRDGarantiaFNDCalculo @Cedula, @Garantia, @Contrato;",
                new
                {
                    Cedula = request.cedula.Trim(),
                    Garantia = request.fnd_garantia.Trim(),
                    Contrato = request.fnd_contrato
                });

            if (request.monto <= disponible)
            {
                return;
            }

            mensajes.Add(request.fnd_contrato > 0
                ? " - El Monto Solicitado excede la cobertura de su PLAN DE INVERSIÓN..."
                : " - El Monto Solicitado excede la cobertura de sus PLANES de ahorros...");
        }

        private static void Cr_SeguimientoTramites_Recepcion_CobroJudicial_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesRecepcionGuardarRequest request,
            int permiteCobroJudicial,
            ICollection<string> mensajes)
        {
            if (permiteCobroJudicial != 0)
            {
                return;
            }

            const string sql = """
                select count(1)
                from reg_creditos
                where estado = 'A' and proceso = 'J' and cedula = @Cedula;
                """;

            if (conn.QuerySingle<int>(sql, new { Cedula = request.cedula.Trim() }) > 0)
            {
                mensajes.Add("- La persona tiene créditos en Cobro Judicial");
            }
        }
    }
}
