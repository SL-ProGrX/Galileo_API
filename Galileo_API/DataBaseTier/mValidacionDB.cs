using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models;

namespace Galileo_API.DataBaseTier
{
    public sealed class MValidacionDb
    {
        private readonly PortalDB _portalDB;

        public MValidacionDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la parte de una llave primaria ubicada antes
        /// del texto delimitador.
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="inicioCodigo"></param>
        /// <param name="buscar"></param>
        /// <returns></returns>
        public static string FxDeCodificaPrimaryKey_Obtener(
            string? primaryKey,
            int inicioCodigo,
            string? buscar)
        {
            if (string.IsNullOrEmpty(primaryKey) ||
                string.IsNullOrEmpty(buscar) ||
                inicioCodigo < 1)
            {
                return string.Empty;
            }

            int posicion = primaryKey.IndexOf(
                buscar,
                StringComparison.OrdinalIgnoreCase);

            if (posicion < 0 ||
                inicioCodigo - 1 > posicion)
            {
                return string.Empty;
            }

            return primaryKey[
                (inicioCodigo - 1)..posicion
            ].Trim();
        }

        /// <summary>
        /// Obtiene las garantias asociadas a una linea
        /// de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            SbSTCargaCboGarantia_Obtener(
                int codEmpresa,
                string? linea)
        {
            string lineaNormalizada =
                linea?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(lineaNormalizada))
            {
                return DbHelper.CreateOkResponse(
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = """
            select
                trim(T.garantia) as item,
                trim(T.descripcion) as descripcion
            from crd_catalogo_garantias C
            inner join crd_garantia_tipos T
                on C.garantia = T.garantia
            where C.codigo = @Linea
            order by T.descripcion;
            """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDB,
                    codEmpresa,
                    sql,
                    new
                    {
                        Linea = lineaNormalizada
                    });
        }

        /// <summary>
        /// Obtiene los destinos asociados a una linea
        /// de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<List<MValidacionDestino>>
            SbSTCargaCboDestinos_Obtener(
                int codEmpresa,
                string? linea)
        {
            string lineaNormalizada =
                linea?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(lineaNormalizada))
            {
                return DbHelper.CreateOkResponse(
                    new List<MValidacionDestino>());
            }

            const string sql = """
                select
                    trim(D.cod_destino) as cod_destino,
                    trim(D.descripcion) as descripcion,
                    trim(D.cod_destino) + ' - ' +
                        trim(D.descripcion) as campo
                from catalogo_destinos D
                inner join catalogo_destinosASG C
                    on D.cod_destino = C.cod_destino
                where C.codigo = @Linea
                order by D.prioridad;
                """;

            return DbHelper.ExecuteListQuery<MValidacionDestino>(
                _portalDB,
                codEmpresa,
                sql,
                new
                {
                    Linea = lineaNormalizada
                });
        }

        /// <summary>
        /// Convierte el codigo de garantia en descripcion
        /// o la descripcion en codigo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<string?>
            FxGarantia_Obtener(
                int codEmpresa,
                string? tipo)
        {
            string valor =
                tipo?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(valor))
            {
                return DbHelper.CreateOkResponse<string?>(
                    string.Empty);
            }

            const string sqlPorCodigo = """
            select top 1
                trim(descripcion) as resultado
            from crd_garantia_tipos
            where garantia = @Valor;
            """;

            const string sqlPorDescripcion = """
            select top 1
                trim(garantia) as resultado
            from crd_garantia_tipos
            where descripcion = @Valor;
            """;

            string sql = valor.Length <= 3
                ? sqlPorCodigo
                : sqlPorDescripcion;

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                codEmpresa,
                sql,
                string.Empty,
                new
                {
                    Valor = valor
                });
        }

        /// <summary>
        /// Calcula la bonificacion por membresia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            FxBonoMembresia_Obtener(
                int codEmpresa,
                MValidacionBonoMembresiaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string cedula = request.cedula.Trim();
            string linea = request.linea.Trim();
            string garantia = request.garantia.Trim();
            string destino = request.destino.Trim();

            if (string.IsNullOrEmpty(cedula) ||
                string.IsNullOrEmpty(linea) ||
                string.IsNullOrEmpty(garantia))
            {
                return DbHelper.CreateOkResponse<decimal?>(0);
            }

            const string sql = """
                select coalesce(
                    fxCrdTasaBonifica(
                        @Cedula,
                        @Linea,
                        @Garantia,
                        @Destino,
                        @Plazo
                    ),
                    0
                ) as resultado;
                """;

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Cedula = cedula,
                    Linea = linea,
                    Garantia = garantia,
                    Destino = destino,
                    Plazo = Math.Max(request.plazo, 1)
                });
        }

        /// <summary>
        /// Obtiene la bonificacion de plazo correspondiente
        /// a la membresia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        public ErrorDto<int?>
            FxBonoPlazoMembresia_Obtener(
                int codEmpresa,
                string? cedula,
                string? garantia)
        {
            string cedulaNormalizada =
                cedula?.Trim() ?? string.Empty;

            string garantiaNormalizada =
                garantia?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(cedulaNormalizada) ||
                string.IsNullOrEmpty(garantiaNormalizada))
            {
                return DbHelper.CreateOkResponse<int?>(0);
            }

            const string sql = """
                select coalesce(
                    fxCrdPlazoBonifica(
                        @Cedula,
                        @Garantia
                    ),
                    0
                ) as resultado;
                """;

            return DbHelper.ExecuteSingleQuery<int?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Cedula = cedulaNormalizada,
                    Garantia = garantiaNormalizada
                });
        }

        /// <summary>
        /// Obtiene el valor definido en el catalogo de rangos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            FxCatalogoRango_Obtener(
                int codEmpresa,
                MValidacionCatalogoRangoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            const string sql = """
                select coalesce(
                    fxCrdCatalogoRango(
                        @Codigo,
                        @Monto,
                        @Tipo,
                        @Destino,
                        @Garantia
                    ),
                    0
                ) as resultado;
                """;

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Codigo = request.codigo.Trim(),
                    request.monto,
                    Tipo = request.tipo.Trim(),
                    Destino =
                        request.cod_destino.Trim(),
                    Garantia =
                        request.garantia.Trim()
                });
        }

        /// <summary>
        /// Obtiene el valor del catalogo correspondiente
        /// al plazo indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            FxCatalogoRangoPlz_Obtener(
                int codEmpresa,
                MValidacionCatalogoRangoPlazoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            const string sql = """
                select coalesce(
                    fxCrdCatalogoRangoPlz(
                        @Codigo,
                        @Plazo,
                        @Destino,
                        @Garantia
                    ),
                    0
                ) as resultado;
                """;

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Codigo = request.codigo.Trim(),
                    request.plazo,
                    Destino =
                        request.cod_destino.Trim(),
                    Garantia =
                        request.garantia.Trim()
                });
        }

        /// <summary>
        /// Calcula la cuota segun el monto, plazo, interes
        /// y frecuencia de pago indicados.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static decimal
            MValidacion_FxCalcula_Cuota_Obtener(
                MValidacionCuotaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.monto <= 0 ||
                request.plazo <= 0)
            {
                return 0;
            }

            decimal interesPeriodo =
                request.frecuencia
                    .Trim()
                    .ToUpperInvariant() switch
                {
                    "M" => request.interes / (12m * 100m),
                    "Q" => request.interes / (24m * 100m),
                    _ => request.interes / (12m * 100m)
                };

            if (interesPeriodo == 0)
            {
                return Math.Round(
                    request.monto / request.plazo,
                    2,
                    MidpointRounding.ToEven);
            }

            decimal factor = Convert.ToDecimal(
                Math.Pow(
                    1d + (double)interesPeriodo,
                    request.plazo));

            decimal denominador = factor - 1m;

            if (denominador == 0)
            {
                return 0;
            }

            decimal cuota =
                request.monto *
                interesPeriodo *
                factor /
                denominador;

            return Math.Round(
                cuota,
                2,
                MidpointRounding.ToEven);
        }

        /// <summary>
        /// Obtiene el valor correspondiente al parametro indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codParametro"></param>
        /// <returns></returns>
        public ErrorDto<string?>
            FxTraerValorParametro_Obtener(
                int codEmpresa,
                string? codParametro)
        {
            string codigo =
                codParametro?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(codigo))
            {
                return DbHelper.CreateOkResponse<string?>(
                    string.Empty);
            }

            const string sql = "exec spCRDPreaPARAMETROS_TVVALOR @Cod_Parametro;";

            var consulta =
                DbHelper.ExecuteSingleQuery<string>(
                    _portalDB,
                    codEmpresa,
                    sql,
                    string.Empty,
                    new
                    {
                        Cod_Parametro = codigo
                    });

            if (consulta.Code != 0)
            {
                return consulta;
            }

            string valor =
                consulta.Result?.Trim() ??
                string.Empty;

            return DbHelper.CreateOkResponse<string?>(
                valor == "-1"
                    ? string.Empty
                    : valor);
        }

        /// <summary>
        /// Calcula la antigüedad desde la fecha indicada
        /// utilizando la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> FxMembresia_Obtener(
            int codEmpresa,
            DateTime fecha)
        {
            const string sql = "select Getdate() as resultado;";
            var consulta =
                DbHelper.ExecuteSingleQuery<DateTime?>(
                    _portalDB,
                    codEmpresa,
                    sql);

            if (consulta.Code != 0 ||
                !consulta.Result.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    consulta.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    consulta.Code.GetValueOrDefault(-1),
                    "Membresia no valida");
            }

            int dias = (
                consulta.Result.Value.Date -
                fecha.Date
            ).Days;

            if (dias < 0)
            {
                return DbHelper.CreateOkResponse(
                    "Membresia no valida");
            }

            int anios = 0;
            int meses = 0;

            while (dias > 365)
            {
                anios++;
                dias -= 365;
            }

            while (dias > 30)
            {
                meses++;
                dias -= 30;
            }

            var partes = new List<string>();

            if (anios > 0)
            {
                partes.Add($"{anios} año(s)");
            }

            if (meses > 0)
            {
                partes.Add($"{meses} mes(es)");
            }

            string resultado =
                string.Join(", ", partes);

            if (dias > 0)
            {
                if (!string.IsNullOrEmpty(resultado))
                {
                    resultado += " con ";
                }

                resultado += $"{dias} dia(s) ";
            }

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// Obtiene los fondos disponibles para la cedula
        /// y garantia indicadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            FxDisponibleFondos_Obtener(
                int codEmpresa,
                string? cedula,
                string? garantia)
        {
            string cedulaNormalizada =
                cedula?.Trim() ?? string.Empty;

            string garantiaNormalizada =
                garantia?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(cedulaNormalizada) ||
                string.IsNullOrEmpty(garantiaNormalizada))
            {
                return DbHelper.CreateOkResponse<decimal?>(0);
            }

            const string sql = "exec spCRDGarantiaFNDCalculo @Cedula, @Garantia;";

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Cedula = cedulaNormalizada,
                    Garantia = garantiaNormalizada
                });
        }
    }
}