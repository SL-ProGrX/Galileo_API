using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCalculoOperacionDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrCalculoOperacionDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCrCalculoOperacionDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene la informacion inicial del socio, garantia sobre ahorros y refundiciones activas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrCalculoOperacionPantallaData> CrCalculoOperacion_Cedula_Obtener(
            int codEmpresa,
            string cedula)
        {
            cedula = LimpiarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return new ErrorDto<CrCalculoOperacionPantallaData>
                {
                    Code = -1,
                    Description = "Debe indicar la cedula."
                };
            }

            var garantiaResp = ObtenerGarantiaAhorro(codEmpresa, cedula);
            if (garantiaResp.Code < 0)
                return garantiaResp;

            var refundicionesResp = ObtenerRefundiciones(codEmpresa, cedula);
            if (refundicionesResp.Code < 0)
            {
                return new ErrorDto<CrCalculoOperacionPantallaData>
                {
                    Code = refundicionesResp.Code,
                    Description = refundicionesResp.Description
                };
            }

            garantiaResp.Result.refundiciones = refundicionesResp.Result;

            return garantiaResp;
        }

        /// <summary>
        /// Obtiene la informacion de la linea seleccionada y sus cargos automaticos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCalculoOperacionCodigoData> CrCalculoOperacion_Codigo_Obtener(
            int codEmpresa,
            string cedula,
            string codigo)
        {
            cedula = LimpiarTexto(cedula);
            codigo = LimpiarCodigo(codigo);

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return new ErrorDto<CrCalculoOperacionCodigoData>
                {
                    Code = -1,
                    Description = "Debe indicar la cedula."
                };
            }

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCalculoOperacionCodigoData>
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de linea."
                };
            }

            var lineaResp = ObtenerLinea(codEmpresa, codigo);
            if (lineaResp.Code < 0)
            {
                return new ErrorDto<CrCalculoOperacionCodigoData>
                {
                    Code = lineaResp.Code,
                    Description = lineaResp.Description
                };
            }

            var cargosResp = ObtenerCargosAutomaticos(codEmpresa, codigo);
            if (cargosResp.Code < 0)
            {
                return new ErrorDto<CrCalculoOperacionCodigoData>
                {
                    Code = cargosResp.Code,
                    Description = cargosResp.Description
                };
            }

            var codigoTipo = ObtenerTipoCodigo(codEmpresa, codigo);
            var dias = ObtenerDiasInteres(codEmpresa, lineaResp.Result);
            var frecuenciaPago = lineaResp.Result.base_calculo == "06" ? "Q" : "M";
            var montoSolicitado = ObtenerMontoSolicitadoInicial(codEmpresa, cedula, codigoTipo);
            var rangoMaximo = FxRangoMaximo(codEmpresa, codigo);

            return new ErrorDto<CrCalculoOperacionCodigoData>
            {
                Code = 0,
                Result = new CrCalculoOperacionCodigoData
                {
                    resumen = new CrCalculoOperacionResumenData
                    {
                        cedula = cedula,
                        codigo = codigo,
                        descripcion = lineaResp.Result.descripcion,
                        base_calculo = lineaResp.Result.base_calculo,
                        frecuencia_pago = frecuenciaPago,
                        dias = dias,
                        codigo_tipo = codigoTipo,
                        monto_solicitado = montoSolicitado,
                        rango_maximo = rangoMaximo,
                        refunde = lineaResp.Result.refunde,
                        operaciones_activas = lineaResp.Result.operaciones_activas
                    },
                    cargos = cargosResp.Result ?? new List<CrCalculoOperacionCargoData>()
                }
            };
        }

        /// <summary>
        /// Obtiene plazo y tasa segun linea y monto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        public ErrorDto<CrCalculoOperacionRangosData> CrCalculoOperacion_Rangos_Obtener(
            int codEmpresa,
            string codigo,
            decimal monto)
        {
            codigo = LimpiarCodigo(codigo);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCalculoOperacionRangosData>
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de linea."
                };
            }

            return new ErrorDto<CrCalculoOperacionRangosData>
            {
                Code = 0,
                Result = new CrCalculoOperacionRangosData
                {
                    plazo = Convert.ToInt32(FxCatalogoRango(codEmpresa, codigo, monto, "P")),
                    tasa = FxCatalogoRango(codEmpresa, codigo, monto, "I")
                }
            };
        }

        /// <summary>
        /// Obtiene los disponibles por garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCalculoOperacionDisponibleData>> CrCalculoOperacion_Disponibles_Obtener(
            int codEmpresa,
            string cedula)
        {
            cedula = LimpiarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return new ErrorDto<List<CrCalculoOperacionDisponibleData>>
                {
                    Code = -1,
                    Description = "Debe indicar la cedula."
                };
            }

            var list = new List<CrCalculoOperacionDisponibleData>();

            var ahorros = ObtenerDisponibleAhorros(codEmpresa, cedula);
            if (ahorros != null)
            {
                list.Add(new CrCalculoOperacionDisponibleData
                {
                    garantia = "Sobre Ahorros",
                    monto = ahorros.disponible,
                    saldo = ahorros.saldos,
                    disponible = ahorros.disponible - ahorros.saldos
                });
            }

            var fiduciario = ObtenerDisponibleFiduciario(codEmpresa, cedula);
            if (fiduciario != null)
            {
                list.Add(new CrCalculoOperacionDisponibleData
                {
                    garantia = "Fiduciaria",
                    monto = fiduciario.disponible,
                    saldo = fiduciario.saldos,
                    disponible = fiduciario.disponible - fiduciario.saldos
                });
            }

            var excedente = ObtenerDisponibleExcedente(codEmpresa, cedula);
            if (excedente != null)
            {
                list.Add(new CrCalculoOperacionDisponibleData
                {
                    garantia = "Excedentes",
                    monto = excedente.base_credito,
                    saldo = excedente.saldos,
                    disponible = excedente.base_credito - excedente.saldos
                });
            }

            return new ErrorDto<List<CrCalculoOperacionDisponibleData>>
            {
                Code = 0,
                Result = list
            };
        }

        /// <summary>
        /// Obtiene la lista de lineas del catalogo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCalculoOperacion_Catalogo_Obtener(int codEmpresa)
        {
            const string sql = @"
            select
                rtrim(Codigo) as item,
                rtrim(Descripcion) as descripcion
            from catalogo
            order by Codigo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene la garantia sobre ahorros del socio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private ErrorDto<CrCalculoOperacionPantallaData> ObtenerGarantiaAhorro(int codEmpresa, string cedula)
        {
            const string sql = @"exec spCrdGarantiaPatDetalle @Cedula;";

            var resp = DbHelper.ExecuteListQuery<GarantiaPatDetalleQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = cedula });

            if (resp.Code < 0)
            {
                return new ErrorDto<CrCalculoOperacionPantallaData>
                {
                    Code = resp.Code,
                    Description = resp.Description
                };
            }

            var item = resp.Result?.FirstOrDefault();
            if (item == null)
            {
                return new ErrorDto<CrCalculoOperacionPantallaData>
                {
                    Code = -1,
                    Description = "No se encontro informacion del socio."
                };
            }

            return new ErrorDto<CrCalculoOperacionPantallaData>
            {
                Code = 0,
                Result = new CrCalculoOperacionPantallaData
                {
                    resumen = new CrCalculoOperacionResumenData
                    {
                        cedula = cedula,
                        nombre = item.nombre
                    },
                    garantia_ahorro = new CrCalculoOperacionGarantiaAhorroData
                    {
                        porcentaje_obrero = item.porc_obrero,
                        porcentaje_patronal = item.porc_patronal,
                        porcentaje_capitaliza = item.porc_capitaliza,
                        aporte_obrero = item.mnt_obrero,
                        aporte_patronal = item.mnt_patronal,
                        capitalizacion = item.mnt_capitaliza,
                        disponible_bruto = item.monto
                    },
                    refundiciones = new List<CrCalculoOperacionRefundicionData>()
                }
            };
        }

        /// <summary>
        /// Obtiene las refundiciones activas del socio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private ErrorDto<List<CrCalculoOperacionRefundicionData>> ObtenerRefundiciones(int codEmpresa, string cedula)
        {
            const string sql = @"
            select
                R.id_solicitud as operacion,
                rtrim(R.codigo) as codigo,
                isnull(R.saldo,0) as saldo_real,
                rtrim(isnull(R.garantia,'')) as garantia,
                rtrim(isnull(Gar.Descripcion,'')) as garantia_descripcion,
                isnull(V.intc,0) as mora_intc,
                isnull(V.intm,0) as mora_intm,
                isnull(V.amortiza,0) as mora_principal,
                isnull(R.cuota,0) as cuota,
                isnull(R.amortiza,0) as recaudado,
                isnull(R.plazo,0) as plazo,
                isnull(R.montoapr,0) as montoapr,
                isnull(C.retencion,'N') as retencion,
                isnull(C.poliza,'N') as poliza,
                isnull(C.aceptarefun,'N') as aceptarefun,
                isnull(C.refunde_tipo,'') as refunde_tipo,
                isnull(C.refunde_porc,0) as refunde_porc,
                isnull(
                    datediff(m, dbo.fxSIFCorteAFechaInicio(R.PRIDEDUC), GETDATE()) / convert(float,R.PLAZO)
                ,0) as tiempo_transcurrido
            from reg_creditos R
            inner join Catalogo C on R.codigo = C.codigo
            inner join crd_garantia_tipos Gar on R.garantia = Gar.Garantia
            left join Vista_morosidad V on R.id_solicitud = V.id_solicitud
            where R.cedula = @Cedula
              and R.saldo > 0
              and R.proceso <> 'J'
              and R.estado = 'A';";

            return DbHelper.ExecuteListQuery<CrCalculoOperacionRefundicionData>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene la informacion base de la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private ErrorDto<LineaCodigoQueryDto> ObtenerLinea(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(isnull(descripcion,'')) as descripcion,
                    isnull(fechacortealterna,'N') as fechacortealterna,
                    fechacorte,
                    Getdate() as fecha_server,
                    rtrim(isnull(Base_Calculo,'')) as base_calculo,
                    isnull(refunde,'N') as refunde,
                    isnull(operaciones_activas,0) as operaciones_activas
                from catalogo
                where codigo = @Codigo;";

            var resp = DbHelper.ExecuteListQuery<LineaCodigoQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });

            if (resp.Code < 0)
            {
                return new ErrorDto<LineaCodigoQueryDto>
                {
                    Code = resp.Code,
                    Description = resp.Description
                };
            }

            var item = resp.Result?.FirstOrDefault();
            if (item == null)
            {
                return new ErrorDto<LineaCodigoQueryDto>
                {
                    Code = -1,
                    Description = "No se encontro el codigo especificado."
                };
            }

            return new ErrorDto<LineaCodigoQueryDto>
            {
                Code = 0,
                Result = item
            };
        }

        /// <summary>
        /// Obtiene cargos automaticos asociados a la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private ErrorDto<List<CrCalculoOperacionCargoData>> ObtenerCargosAutomaticos(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(C.COD_CARGO) as cod_cargo,
                    rtrim(isnull(C.Descripcion,'')) as descripcion,
                    rtrim(isnull(C.Tipo,'')) as tipo,
                    isnull(C.Valor,0) as valor
                from cargos_adicionales C
                inner join cargos_asignacion A on C.cod_cargo = A.cod_cargo
                where A.codigo = @Codigo
                  and C.Automatico = 1;";

            return DbHelper.ExecuteListQuery<CrCalculoOperacionCargoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Obtiene los dias de interes segun configuracion de la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        private int ObtenerDiasInteres(int codEmpresa, LineaCodigoQueryDto linea)
        {
            if (linea.fechacortealterna == "S")
            {
                var dias = (linea.fechacorte - linea.fecha_server).Days + 1;
                return dias < 0 ? 0 : dias;
            }

            const string sql = @"select cr_fecha_calculo, Getdate() as fecha from par_ahcr;";

            var resp = DbHelper.ExecuteListQuery<ParAhcrFechaQueryDto>(_portalDb, codEmpresa, sql);
            var item = resp.Result?.FirstOrDefault();

            if (item == null)
                return 0;

            var diasPar = (item.cr_fecha_calculo - item.fecha).Days + 1;
            return diasPar < 0 ? 0 : diasPar;
        }

        /// <summary>
        /// Identifica el tipo especial de linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private int ObtenerTipoCodigo(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(Gar.GARANTIA) as garantia,
                    rtrim(isnull(Gar.FORMULARIO,'')) as formulario
                from CRD_CATALOGO_GARANTIAS Cat
                inner join CRD_GARANTIA_TIPOS Gar on Cat.GARANTIA = Gar.GARANTIA
                where Cat.CODIGO = @Codigo;";

            var resp = DbHelper.ExecuteListQuery<TipoGarantiaQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });

            foreach (var item in resp.Result ?? new List<TipoGarantiaQueryDto>())
            {
                switch (item.formulario)
                {
                    case "F01":
                        return 2;
                    case "F08":
                        return 1;
                    case "F06":
                        return 3;
                }
            }

            if (EsCodigoExcedente(codEmpresa, codigo))
                return 1;

            return 0;
        }

        /// <summary>
        /// Obtiene el monto inicial sugerido segun tipo de linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codigoTipo"></param>
        /// <returns></returns>
        private decimal ObtenerMontoSolicitadoInicial(int codEmpresa, string cedula, int codigoTipo)
        {
            return codigoTipo switch
            {
                1 => ObtenerExcedenteDisponible(codEmpresa, cedula),
                3 => ObtenerDisponibleFondos(codEmpresa, cedula, string.Empty, 0),
                _ => 0m
            };
        }

        /// <summary>
        /// Valida si la linea es de excedentes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private bool EsCodigoExcedente(int codEmpresa, string codigo)
        {
            const string sqlExcParametros = @"
                select rtrim(isnull(valor,'')) as codigo
                from EXC_PARAMETROS
                where COD_PARAMETRO = '05';";

            var excResp = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sqlExcParametros,
                string.Empty);

            if ((excResp.Result ?? string.Empty).Trim().Equals(codigo, StringComparison.OrdinalIgnoreCase))
                return true;

            const string sqlExcedentesParametros = @"select rtrim(isnull(ase_codigo,'')) from excedentes_parametros;";

            var excedenteResp = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sqlExcedentesParametros,
                string.Empty);

            return (excedenteResp.Result ?? string.Empty).Trim()
                .Equals(codigo, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el disponible base de excedentes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private decimal ObtenerExcedenteDisponible(int codEmpresa, string cedula)
        {
            var item = ObtenerDisponibleExcedente(codEmpresa, cedula);
            return item?.base_credito ?? 0;
        }

        /// <summary>
        /// Obtiene el disponible de fondos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="garantia"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        private decimal ObtenerDisponibleFondos(int codEmpresa, string cedula, string garantia, int contrato)
        {
            const string sql = @"exec spCRDGarantiaFNDCalculo @Cedula, @Garantia, @Contrato;";

            var resp = DbHelper.ExecuteListQuery<DisponibleFondosQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Cedula = cedula,
                    Garantia = garantia,
                    Contrato = contrato
                });

            return resp.Result?.FirstOrDefault()?.disponible ?? 0;
        }

        /// <summary>
        /// Obtiene el rango de linea segun monto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="monto"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private decimal FxCatalogoRango(int codEmpresa, string codigo, decimal monto, string tipo)
        {
            const string sql = @"select dbo.fxCrdCatalogoRango(@Codigo,@Monto,@Tipo,'','') as resultado;";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    Codigo = codigo,
                    Monto = monto,
                    Tipo = tipo
                }).Result;
        }

        /// <summary>
        /// Obtiene el maximo de rangos para la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private decimal FxRangoMaximo(int codEmpresa, string codigo)
        {
            const string sql = @"select isnull(max(hasta),0) from rangos where codigo = @Codigo;";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Codigo = codigo }).Result;
        }

        /// <summary>
        /// Obtiene disponible sobre ahorros.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private DisponibleBaseQueryDto? ObtenerDisponibleAhorros(int codEmpresa, string cedula)
        {
            const string sql = @"exec spVoxAhorros @Cedula;";

            return DbHelper.ExecuteListQuery<DisponibleBaseQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = cedula }).Result?.FirstOrDefault();
        }

        /// <summary>
        /// Obtiene disponible fiduciario.
        /// </summary>
        private DisponibleBaseQueryDto? ObtenerDisponibleFiduciario(int codEmpresa, string cedula)
        {
            const string sql = @"exec spVoxFiduciario @Cedula;";

            return DbHelper.ExecuteListQuery<DisponibleBaseQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = cedula }).Result?.FirstOrDefault();
        }

        /// <summary>
        /// Obtiene disponible de excedentes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private DisponibleExcedenteQueryDto? ObtenerDisponibleExcedente(int codEmpresa, string cedula)
        {
            const string sql = @"exec spVoxExcedenteCredito @Cedula;";

            return DbHelper.ExecuteListQuery<DisponibleExcedenteQueryDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = cedula }).Result?.FirstOrDefault();
        }

        private static string LimpiarTexto(string valor)
            => (valor ?? string.Empty).Trim();

        private static string LimpiarCodigo(string valor)
            => (valor ?? string.Empty).Trim().ToUpperInvariant();

        private sealed class GarantiaPatDetalleQueryDto
        {
            public string nombre { get; set; } = string.Empty;
            public decimal porc_obrero { get; set; } = 0;
            public decimal porc_patronal { get; set; } = 0;
            public decimal porc_capitaliza { get; set; } = 0;
            public decimal mnt_patronal { get; set; } = 0;
            public decimal mnt_obrero { get; set; } = 0;
            public decimal mnt_capitaliza { get; set; } = 0;
            public decimal monto { get; set; } = 0;
        }

        private sealed class LineaCodigoQueryDto
        {
            public string descripcion { get; set; } = string.Empty;
            public string fechacortealterna { get; set; } = "N";
            public DateTime fechacorte { get; set; } = DateTime.Now;
            public DateTime fecha_server { get; set; } = DateTime.Now;
            public string base_calculo { get; set; } = string.Empty;
            public string refunde { get; set; } = "N";
            public int operaciones_activas { get; set; } = 0;
        }

        private sealed class ParAhcrFechaQueryDto
        {
            public DateTime cr_fecha_calculo { get; set; } = DateTime.Now;
            public DateTime fecha { get; set; } = DateTime.Now;
        }

        private sealed class TipoGarantiaQueryDto
        {
            public string garantia { get; set; } = string.Empty;
            public string formulario { get; set; } = string.Empty;
        }

        private sealed class DisponibleBaseQueryDto
        {
            public decimal disponible { get; set; } = 0;
            public decimal saldos { get; set; } = 0;
        }

        private sealed class DisponibleExcedenteQueryDto
        {
            public decimal base_credito { get; set; } = 0;
            public decimal saldos { get; set; } = 0;
        }

        private sealed class DisponibleFondosQueryDto
        {
            public decimal disponible { get; set; } = 0;
        }
    }
}