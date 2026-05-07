using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Extensions.Configuration;

namespace Galileo_API.DataBaseTier
{
    public class mCntX_Modulo
    {
        private readonly PortalDB _portalDb;

        public mCntX_Modulo(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los parametros de la contabilidad seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CntXParametrosDto> sbCntX_Contabilidad_Selecciona(int codEmpresa, int codContabilidad)
        {
            const string sqlContabilidad = @"
                select cod_contabilidad as CodigoConta,
                       rtrim(nombre) as NombreEmpresa,
                       nivel1,
                       nivel2,
                       nivel3,
                       nivel4,
                       nivel5,
                       nivel6,
                       nivel7,
                       nivel8
                from CntX_Contabilidades
                where cod_contabilidad = @CodContabilidad";

            const string sqlPeriodo = @"
                select top 1 rtrim(cod_divisa) as DivisaLocal,
                       dbo.fxCntX_PeriodoActual(@CodContabilidad) as Periodo
                from CNTX_DIVISAS
                where cod_contabilidad = @CodContabilidad
                  and divisa_local = 1";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parametros = conn.QueryFirstOrDefault<CntXParametrosDto>(sqlContabilidad, new { CodContabilidad = codContabilidad });
                if (parametros == null)
                {
                    throw new InvalidOperationException("Esta Contabilidad a sido Eliminada o no tiene Acceso a ella, verifique...");
                }

                parametros.TotalChr = parametros.Nivel1 + parametros.Nivel2 + parametros.Nivel3 + parametros.Nivel4
                    + parametros.Nivel5 + parametros.Nivel6 + parametros.Nivel7 + parametros.Nivel8;
                parametros.Mascara = fxCntX_CuentaMascara(
                    parametros.Nivel1,
                    parametros.Nivel2,
                    parametros.Nivel3,
                    parametros.Nivel4,
                    parametros.Nivel5,
                    parametros.Nivel6,
                    parametros.Nivel7,
                    parametros.Nivel8);
                parametros.MascaraCod = string.Concat(
                    parametros.Nivel1,
                    parametros.Nivel2,
                    parametros.Nivel3,
                    parametros.Nivel4,
                    parametros.Nivel5,
                    parametros.Nivel6,
                    parametros.Nivel7,
                    parametros.Nivel8);

                var periodo = conn.QueryFirstOrDefault<CntXPeriodoDivisaDto>(sqlPeriodo, new { CodContabilidad = codContabilidad });
                if (periodo?.Periodo != null)
                {
                    parametros.PeriodoAnio = periodo.Periodo.Value.Year;
                    parametros.PeriodoMes = periodo.Periodo.Value.Month;
                    parametros.DivisaLocal = periodo.DivisaLocal;
                }

                return parametros;
            });
        }

        /// <summary>
        /// Guarda el estado de acceso contable del usuario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="periodoAnio"></param>
        /// <param name="periodoMes"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCntX_Estado_Guarda(int codEmpresa, string usuario, int codContabilidad, int periodoAnio, int periodoMes)
        {
            const string sql = @"
                if exists (select 1 from CntX_Acceso_Historico where usuario = @Usuario)
                begin
                    update CntX_Acceso_Historico
                    set cod_contabilidad = @CodContabilidad,
                        mes = @PeriodoMes,
                        anio = @PeriodoAnio
                    where usuario = @Usuario
                end
                else
                begin
                    insert into CntX_Acceso_Historico(usuario, anio, mes, cod_contabilidad)
                    values(@Usuario, @PeriodoAnio, @PeriodoMes, @CodContabilidad)
                end";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new
                {
                    Usuario = usuario,
                    CodContabilidad = codContabilidad,
                    PeriodoAnio = periodoAnio,
                    PeriodoMes = periodoMes
                });
                return true;
            });
        }

        /// <summary>
        /// Valida que exista el registro base del modulo contable.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCntX_ParametrosIniciales(int codEmpresa)
        {
            const string sql = "select isnull(count(1), 0) from CntX_Empresa_Registro";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                int total = conn.ExecuteScalar<int>(sql);
                if (total == 0)
                {
                    throw new InvalidOperationException("No se ha registrado el Sistema Correctamente...(Registrar y Reiniciar)");
                }

                return true;
            });
        }

        /// <summary>
        /// Obtiene las unidades de negocio del modulo contable.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbCntX_CargaCboUnidades(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                select cast('' as varchar(10)) as item, '[CONSOLIDADO]' as descripcion
                union all
                select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion
                from CntX_Unidades
                where cod_contabilidad = @CodContabilidad";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Homologa el tipo de cuenta entre codigo y descripcion.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static string fxCntX_TiposCuentas(string tipo)
        {
            return tipo.Trim().ToUpperInvariant() switch
            {
                "I" => "INGRESOS",
                "G" => "GASTOS",
                "C" => "CAPITAL/PATRIMONIO",
                "P" => "PASIVOS",
                "A" => "ACTIVOS",
                "O" => "ORDEN - DEUDORAS",
                "Q" => "ORDEN - ACREEDORAS",
                "INGRESOS" => "I",
                "GASTOS" => "G",
                "CAPITAL/PATRIMONIO" => "C",
                "CAPITAL" => "C",
                "PATRIMONIO" => "C",
                "PASIVOS" => "P",
                "ACTIVOS" => "A",
                "ORDEN - DEUDORAS" => "O",
                "ORDEN - ACREEDORAS" => "Q",
                _ => tipo
            };
        }

        /// <summary>
        /// Genera la mascara de cuenta contable.
        /// </summary>
        /// <param name="nivel1"></param>
        /// <param name="nivel2"></param>
        /// <param name="nivel3"></param>
        /// <param name="nivel4"></param>
        /// <param name="nivel5"></param>
        /// <param name="nivel6"></param>
        /// <param name="nivel7"></param>
        /// <param name="nivel8"></param>
        /// <param name="caracter"></param>
        /// <returns></returns>
        public static string fxCntX_CuentaMascara(int nivel1, int nivel2, int nivel3, int nivel4, int nivel5, int nivel6, int nivel7, int nivel8, string caracter = "#")
        {
            var niveles = new[] { nivel1, nivel2, nivel3, nivel4, nivel5, nivel6, nivel7, nivel8 };
            var partes = niveles.Where(nivel => nivel > 0).Select(nivel => string.Concat(Enumerable.Repeat(caracter, nivel)));
            return string.Join("-", partes);
        }

        /// <summary>
        /// Aplica o remueve la mascara de una cuenta contable.
        /// </summary>
        /// <param name="aplicaMascara"></param>
        /// <param name="cuenta"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public static string fxCntX_CuentaFormato(bool aplicaMascara, string cuenta, CntXParametrosDto parametros)
        {
            return fxCntX_CuentaFormato(
                aplicaMascara,
                cuenta,
                parametros.Nivel1,
                parametros.Nivel2,
                parametros.Nivel3,
                parametros.Nivel4,
                parametros.Nivel5,
                parametros.Nivel6,
                parametros.Nivel7,
                parametros.Nivel8);
        }

        /// <summary>
        /// Aplica o remueve la mascara de una cuenta contable.
        /// </summary>
        /// <param name="aplicaMascara"></param>
        /// <param name="cuenta"></param>
        /// <param name="nivel1"></param>
        /// <param name="nivel2"></param>
        /// <param name="nivel3"></param>
        /// <param name="nivel4"></param>
        /// <param name="nivel5"></param>
        /// <param name="nivel6"></param>
        /// <param name="nivel7"></param>
        /// <param name="nivel8"></param>
        /// <returns></returns>
        public static string fxCntX_CuentaFormato(bool aplicaMascara, string cuenta, int nivel1, int nivel2, int nivel3, int nivel4, int nivel5, int nivel6, int nivel7, int nivel8)
        {
            var niveles = new[] { nivel1, nivel2, nivel3, nivel4, nivel5, nivel6, nivel7, nivel8 };
            string cuentaLimpia = (cuenta ?? string.Empty).Trim().Replace("-", string.Empty);
            if (string.IsNullOrWhiteSpace(cuentaLimpia))
            {
                return string.Empty;
            }

            if (!cuentaLimpia.All(char.IsDigit))
            {
                return cuentaLimpia;
            }

            int total = niveles.Sum();
            cuentaLimpia = cuentaLimpia.PadRight(total, '0');
            if (!aplicaMascara)
            {
                return cuentaLimpia;
            }

            int posicion = 0;
            var partes = new List<string>();
            foreach (int nivel in niveles.Where(nivel => nivel > 0))
            {
                partes.Add(cuentaLimpia.Substring(posicion, nivel));
                posicion += nivel;
            }

            return string.Join("-", partes);
        }

        /// <summary>
        /// Obtiene la descripcion de un mes contable.
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static string fxCntX_MesDesc(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Setiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                13 => "Cierre Fiscal",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene el ultimo dia de un mes.
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="anio"></param>
        /// <returns></returns>
        public static int fxCntX_UltimoDiaMes(int mes, int anio)
        {
            return DateTime.DaysInMonth(anio, mes);
        }

        /// <summary>
        /// Obtiene la descripcion del periodo contable.
        /// </summary>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static string fxCntX_PeriodoDesc(int anio, int mes)
        {
            string mesDesc = fxCntX_MesDesc(mes);
            return mes == 13 ? $"CIERRE FISCAL {anio}" : $"{mesDesc.ToUpperInvariant()} DE {anio}";
        }

        /// <summary>
        /// Obtiene la descripcion del periodo para informes.
        /// </summary>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static string fxCntX_PeriodoDesc_Informes(int anio, int mes)
        {
            if (mes == 13)
            {
                return $"Periodo Fiscal {anio}";
            }

            string mesDesc = fxCntX_MesDesc(mes).ToLowerInvariant();
            int ultimoDia = fxCntX_UltimoDiaMes(mes, anio);
            return $"Del 1 al {ultimoDia} de {mesDesc} de {anio}";
        }

        /// <summary>
        /// Valida si una cuenta existe y acepta movimientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<bool> fxCntX_CuentaValida(int codEmpresa, int codContabilidad, string cuenta)
        {
            const string sql = @"
                select count(1)
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta
                  and acepta_movimientos = 1";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.ExecuteScalar<int>(sql, new { CodContabilidad = codContabilidad, Cuenta = NormalizarCuenta(cuenta) }) > 0);
        }

        /// <summary>
        /// Obtiene la divisa asociada a una cuenta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_CuentaDivisa(int codEmpresa, int codContabilidad, string cuenta)
        {
            const string sql = @"
                select rtrim(cod_divisa)
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta";
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Cuenta = NormalizarCuenta(cuenta) });
        }

        /// <summary>
        /// Obtiene cuenta o descripcion segun el codigo solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigo"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_Cuenta(int codEmpresa, int codContabilidad, string codigo, string parametro)
        {
            const string sqlCuenta = @"
                select rtrim(cod_cuenta)
                from CntX_Cuentas
                where descripcion = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            const string sqlDescripcion = @"
                select rtrim(descripcion)
                from CntX_Cuentas
                where cod_cuenta = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            string sql = string.Equals(codigo, "C", StringComparison.OrdinalIgnoreCase) ? sqlCuenta : sqlDescripcion;
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Parametro = parametro });
        }

        /// <summary>
        /// Obtiene unidad o descripcion segun el codigo solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigo"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_Unidad(int codEmpresa, int codContabilidad, string codigo, string parametro)
        {
            const string sqlUnidad = @"
                select rtrim(cod_unidad)
                from CntX_Unidades
                where descripcion = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            const string sqlDescripcion = @"
                select rtrim(descripcion)
                from CntX_Unidades
                where cod_unidad = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            string sql = string.Equals(codigo, "C", StringComparison.OrdinalIgnoreCase) ? sqlUnidad : sqlDescripcion;
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Parametro = parametro });
        }

        /// <summary>
        /// Obtiene centro de costo o descripcion segun el codigo solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigo"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_CentroCosto(int codEmpresa, int codContabilidad, string codigo, string parametro)
        {
            const string sqlCentro = @"
                select rtrim(cod_centro_costo)
                from CntX_Centro_Costos
                where descripcion = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            const string sqlDescripcion = @"
                select rtrim(descripcion)
                from CntX_Centro_Costos
                where cod_centro_costo = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            string sql = string.Equals(codigo, "C", StringComparison.OrdinalIgnoreCase) ? sqlCentro : sqlDescripcion;
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Parametro = parametro });
        }

        /// <summary>
        /// Obtiene divisa o descripcion segun el codigo solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigo"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_Divisas(int codEmpresa, int codContabilidad, string codigo, string parametro)
        {
            const string sqlDivisa = @"
                select rtrim(cod_divisa)
                from CntX_Divisas
                where descripcion = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            const string sqlDescripcion = @"
                select rtrim(descripcion)
                from CntX_Divisas
                where cod_divisa = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            string sql = string.Equals(codigo, "C", StringComparison.OrdinalIgnoreCase) ? sqlDivisa : sqlDescripcion;
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Parametro = parametro });
        }

        /// <summary>
        /// Obtiene tipo de asiento o descripcion segun el codigo solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codigo"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_TiposAsientos(int codEmpresa, int codContabilidad, string codigo, string parametro)
        {
            const string sqlTipo = @"
                select rtrim(tipo_asiento)
                from CntX_Tipos_Asientos
                where descripcion = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            const string sqlDescripcion = @"
                select rtrim(descripcion)
                from CntX_Tipos_Asientos
                where tipo_asiento = @Parametro
                  and cod_contabilidad = @CodContabilidad";

            string sql = string.Equals(codigo, "C", StringComparison.OrdinalIgnoreCase) ? sqlTipo : sqlDescripcion;
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Parametro = parametro });
        }

        /// <summary>
        /// Obtiene la clasificacion de una cuenta contable.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<string> fxCntX_Cuenta_Clasificacion(int codEmpresa, int codContabilidad, string cuenta)
        {
            const string sql = @"
                select rtrim(T.clasificacion)
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on C.cod_contabilidad = T.cod_contabilidad
                 and C.tipo_cuenta = T.tipo_cuenta
                where C.cod_contabilidad = @CodContabilidad
                  and C.cod_cuenta = @Cuenta";
            return ObtenerTexto(codEmpresa, sql, new { CodContabilidad = codContabilidad, Cuenta = NormalizarCuenta(cuenta) });
        }

        /// <summary>
        /// Indica si una divisa es la divisa local.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="divisa"></param>
        /// <returns></returns>
        public ErrorDto<bool> fxCntX_DivisaBase(int codEmpresa, int codContabilidad, string divisa)
        {
            const string sql = @"
                select isnull(divisa_local, 0)
                from CntX_Divisas
                where cod_contabilidad = @CodContabilidad
                  and cod_divisa = @Divisa";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.ExecuteScalar<int?>(sql, new { CodContabilidad = codContabilidad, Divisa = divisa }) == 1);
        }

        /// <summary>
        /// Obtiene el tipo de cambio aplicable para una cuenta y divisa.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="divisa"></param>
        /// <param name="cuenta"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<decimal> fxCntX_TipoCambio(int codEmpresa, int codContabilidad, string divisa, string cuenta, DateTime fecha)
        {
            const string sqlDivisaLocal = @"
                select isnull(divisa_local, 0)
                from CntX_Divisas
                where cod_contabilidad = @CodContabilidad
                  and cod_divisa = @Divisa";

            const string sqlTipoCambio = @"
                select tc_venta as TcVenta,
                       tc_compra as TcCompra
                from CNTX_DIVISAS_TIPO_CAMBIO
                where cod_contabilidad = @CodContabilidad
                  and cod_divisa = @Divisa
                  and @Fecha between inicio and corte";

            const string sqlClasificacion = @"
                select rtrim(T.clasificacion)
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on C.cod_contabilidad = T.cod_contabilidad
                 and C.tipo_cuenta = T.tipo_cuenta
                where C.cod_contabilidad = @CodContabilidad
                  and C.cod_cuenta = @Cuenta";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parametrosBase = new
                {
                    CodContabilidad = codContabilidad,
                    Divisa = divisa
                };
                int? divisaLocal = conn.ExecuteScalar<int?>(sqlDivisaLocal, parametrosBase);
                if (divisaLocal == null)
                {
                    return 0m;
                }

                if (divisaLocal == 1)
                {
                    return 1m;
                }

                var tipoCambio = conn.QueryFirstOrDefault<CntXTipoCambioDto>(sqlTipoCambio, new
                {
                    CodContabilidad = codContabilidad,
                    Divisa = divisa,
                    Fecha = fecha.Date
                });

                if (tipoCambio == null)
                {
                    return 0m;
                }

                string clasificacion = conn.QueryFirstOrDefault<string>(sqlClasificacion, new
                {
                    CodContabilidad = codContabilidad,
                    Cuenta = NormalizarCuenta(cuenta)
                }) ?? string.Empty;

                return string.Equals(clasificacion, "A", StringComparison.OrdinalIgnoreCase)
                    ? tipoCambio.TcVenta
                    : tipoCambio.TcCompra;
            });
        }

        private ErrorDto<string> ObtenerTexto(int codEmpresa, string sql, object parametros)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.QueryFirstOrDefault<string>(sql, parametros) ?? string.Empty);
        }

        private static string NormalizarCuenta(string cuenta)
        {
            return (cuenta ?? string.Empty).Trim().Replace("-", string.Empty);
        }
    }

}
