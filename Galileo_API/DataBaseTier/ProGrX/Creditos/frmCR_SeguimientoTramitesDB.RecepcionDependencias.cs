using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Obtiene las cuentas bancarias asociadas a la persona y banco indicados.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesOpcionItem>>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionBancoCuentasRequest request)
        {
            string cedula = Cr_SeguimientoTramites_Filtro_Normalizar(request.cedula, 20);
            if (string.IsNullOrWhiteSpace(cedula) || request.banco_id <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la persona y el banco.",
                    -2,
                    new List<CrSeguimientoTramitesOpcionItem>());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Cargar(
                    conn,
                    cedula,
                    request.banco_id));
        }

        /// <summary>
        /// Obtiene el formulario y los cálculos que dependen de la garantía seleccionada.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionGarantiaContextoData>
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
        {
            CrSeguimientoTramitesRecepcionGarantiaContextoRequest normalizado =
                Cr_SeguimientoTramites_Recepcion_Garantia_Request_Normalizar(request);
            string? mensaje =
                Cr_SeguimientoTramites_Recepcion_Garantia_Request_Validar(normalizado);

            if (mensaje is not null)
            {
                return DbHelper.CreateErrorResponse(
                    mensaje,
                    -2,
                    new CrSeguimientoTramitesRecepcionGarantiaContextoData());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Cargar(
                    conn,
                    normalizado));
        }

        /// <summary>
        /// Obtiene contratos activos y cálculos del fondo de garantía seleccionado.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionFondoContextoData>
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionFondoContextoRequest request)
        {
            string cedula = Cr_SeguimientoTramites_Filtro_Normalizar(request.cedula, 20);
            string plan = Cr_SeguimientoTramites_Filtro_Normalizar(request.fnd_garantia, 20);
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(plan))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la persona y el plan de garantía.",
                    -2,
                    new CrSeguimientoTramitesRecepcionFondoContextoData());
            }

            if (request.fnd_contrato < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El contrato del fondo no es válido.",
                    -2,
                    new CrSeguimientoTramitesRecepcionFondoContextoData());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Cargar(
                    conn,
                    cedula,
                    plan,
                    request.fnd_contrato));
        }

        private static List<CrSeguimientoTramitesOpcionItem>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Cargar(
                IDbConnection conn,
                string cedula,
                int bancoId)
        {
            IEnumerable<CrSeguimientoTramitesOpcionRaw> cuentas =
                conn.Query<CrSeguimientoTramitesOpcionRaw>(
                    "spSys_Cuentas_Bancarias",
                    new
                    {
                        Identificacion = cedula,
                        BancoId = bancoId,
                        DivisaCheck = 1
                    },
                    commandType: CommandType.StoredProcedure);

            return Cr_SeguimientoTramites_Opciones_Mapear(cuentas);
        }

        private static CrSeguimientoTramitesRecepcionGarantiaContextoRequest
            Cr_SeguimientoTramites_Recepcion_Garantia_Request_Normalizar(
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
        {
            return new CrSeguimientoTramitesRecepcionGarantiaContextoRequest
            {
                cedula = Cr_SeguimientoTramites_Filtro_Normalizar(request.cedula, 20),
                codigo = Cr_SeguimientoTramites_Filtro_Normalizar(request.codigo, 20),
                destino = Cr_SeguimientoTramites_Filtro_Normalizar(request.destino, 20),
                garantia = Cr_SeguimientoTramites_Filtro_Normalizar(request.garantia, 20),
                monto_actual = request.monto_actual,
                plazo = request.plazo
            };
        }

        private static string? Cr_SeguimientoTramites_Recepcion_Garantia_Request_Validar(
            CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cedula)
                || string.IsNullOrWhiteSpace(request.codigo)
                || string.IsNullOrWhiteSpace(request.destino)
                || string.IsNullOrWhiteSpace(request.garantia))
            {
                return "Debe indicar persona, línea, destino y garantía.";
            }

            if (request.monto_actual < 0)
            {
                return "- El Monto Solicitado NO es válido";
            }

            return request.plazo < 0
                ? "- El Plazo Solicitado NO es válido"
                : null;
        }

        private static CrSeguimientoTramitesRecepcionGarantiaContextoData
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Cargar(
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
        {
            const string sqlFormulario = """
                select
                    rtrim(isnull(G.FORMULARIO, '')) as formulario,
                    rtrim(isnull(C.base_calculo, '')) as base_calculo
                from CRD_GARANTIA_TIPOS G
                cross join CATALOGO C
                where G.garantia = @Garantia
                  and C.codigo = @Codigo;
                """;

            CrSeguimientoTramitesRecepcionGarantiaReglaRaw? regla =
                conn.QueryFirstOrDefault<CrSeguimientoTramitesRecepcionGarantiaReglaRaw>(
                    sqlFormulario,
                    new { Garantia = request.garantia, Codigo = request.codigo });

            if (regla is null)
            {
                throw new InvalidOperationException("No existe la garantía o la línea indicada.");
            }

            CrSeguimientoTramitesRecepcionGarantiaCalculoRaw calculo =
                Cr_SeguimientoTramites_Recepcion_Garantia_Calcular(
                    conn,
                    request,
                    regla.formulario);

            return new CrSeguimientoTramitesRecepcionGarantiaContextoData
            {
                formulario = regla.formulario.Trim(),
                monto_sugerido = calculo.monto_sugerido,
                plazo_sugerido = calculo.plazo_bono > 0 ? calculo.plazo_bono : null,
                tasa_pts_bono = calculo.tasa_pts_bono,
                muestra_fondo = string.Equals(
                    request.garantia,
                    "Y",
                    StringComparison.OrdinalIgnoreCase)
                    || string.Equals(regla.formulario, "F05", StringComparison.OrdinalIgnoreCase),
                muestra_vencimiento = string.Equals(
                    regla.base_calculo,
                    "07",
                    StringComparison.Ordinal),
                permite_traslado_salario = true
            };
        }

        private static CrSeguimientoTramitesRecepcionGarantiaCalculoRaw
            Cr_SeguimientoTramites_Recepcion_Garantia_Calcular(
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request,
                string formulario)
        {
            const string sqlAhorros = """
                select
                    dbo.fxCrdGarantiaPatMnt(@Cedula, @Garantia, 'M') as monto_sugerido,
                    dbo.fxCrdTasaBonifica_New(
                        @Cedula, @Codigo, @Garantia, @Destino, @Plazo
                    ) as tasa_pts_bono,
                    dbo.fxCrdPlazoBonifica(@Cedula, @Garantia) as plazo_bono;
                """;
            const string sqlSalario = """
                select
                    dbo.fxCrdDisponibleAdelantoSalario(@Cedula, 'M') as monto_sugerido,
                    dbo.fxCrdTasaBonifica_New(
                        @Cedula, @Codigo, @Garantia, @Destino, @Plazo
                    ) as tasa_pts_bono,
                    dbo.fxCrdPlazoBonifica(@Cedula, @Garantia) as plazo_bono;
                """;
            const string sqlGeneral = """
                select
                    cast(null as decimal(18, 2)) as monto_sugerido,
                    dbo.fxCrdTasaBonifica_New(
                        @Cedula, @Codigo, @Garantia, @Destino, @Plazo
                    ) as tasa_pts_bono,
                    dbo.fxCrdPlazoBonifica(@Cedula, @Garantia) as plazo_bono;
                """;

            string sql = formulario.Trim().ToUpperInvariant() switch
            {
                "F01" => sqlAhorros,
                "F06" => sqlSalario,
                _ => sqlGeneral
            };

            return conn.QueryFirst<CrSeguimientoTramitesRecepcionGarantiaCalculoRaw>(
                sql,
                new
                {
                    Cedula = request.cedula,
                    Codigo = request.codigo,
                    Garantia = request.garantia,
                    Destino = request.destino,
                    Plazo = request.plazo
                });
        }

        private static CrSeguimientoTramitesRecepcionFondoContextoData
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Cargar(
                IDbConnection conn,
                string cedula,
                string plan,
                int contrato)
        {
            const string sql = """
                select
                    convert(varchar(20), cod_contrato) as idx,
                    concat(
                        '[Cnt: ', cod_contrato, '] [Tasa: ', Tasa_Referencia,
                        '] [I: ', Aportes, '] [V: ',
                        convert(varchar(10), isnull(FECHA_CORTE, getdate()), 23),
                        ']'
                    ) as itmx
                from fnd_contratos
                where cod_plan = @Plan
                  and estado = 'A'
                  and cedula = @Cedula;

                declare @ContratoCalculo int = @Contrato;
                if @ContratoCalculo = 0
                begin
                    select top 1 @ContratoCalculo = cod_contrato
                    from fnd_contratos
                    where cod_plan = @Plan
                      and estado = 'A'
                      and cedula = @Cedula
                    order by cod_contrato;
                end;

                exec spCRDGarantiaFNDCalculo @Cedula, @Plan, @ContratoCalculo;
                select isnull(@ContratoCalculo, 0) as contrato_seleccionado;
                """;

            using SqlMapper.GridReader grid = conn.QueryMultiple(
                sql,
                new { Cedula = cedula, Plan = plan, Contrato = contrato });

            List<CrSeguimientoTramitesOpcionItem> contratos =
                Cr_SeguimientoTramites_Opciones_Mapear(
                    grid.Read<CrSeguimientoTramitesOpcionRaw>());
            CrSeguimientoTramitesRecepcionFondoCalculoRaw calculo =
                grid.ReadFirstOrDefault<CrSeguimientoTramitesRecepcionFondoCalculoRaw>()
                ?? new CrSeguimientoTramitesRecepcionFondoCalculoRaw();
            int contratoSeleccionado =
                grid.ReadFirstOrDefault<int>();

            return new CrSeguimientoTramitesRecepcionFondoContextoData
            {
                contratos = contratos,
                contrato_seleccionado = contratoSeleccionado,
                disponible = calculo.disponible,
                tasa = calculo.tasa,
                plazo = calculo.plazo,
                aplica_tasa = calculo.aplica_tasa,
                aplica_plazo = calculo.aplica_plazo
            };
        }
    }
}
