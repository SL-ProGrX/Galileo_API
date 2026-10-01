using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHExcedentesDistribucionDB
    {
        private readonly PortalDB _portalDB;

        public FrmAHExcedentesDistribucionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los períodos disponibles para distribución de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Distribucion_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = """
                    SELECT
                        IdX AS item,
                        ItmX AS descripcion
                    FROM vExc_Periodos
                    ORDER BY IdX DESC;
                    """;

                return conn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            });
        }

        /// <summary>
        /// Obtiene los cortes disponibles para el período seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Distribucion_Cortes_Lista_Obtener( int CodEmpresa, string periodo)
        {
            periodo = (periodo ?? string.Empty).Trim();

            if (!int.TryParse(periodo, out int periodoId) || periodoId <= 0)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -2,
                    Description = "Debe indicar un período válido.",
                    Result = []
                };
            }

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var resultado = conn.Query<CortePeriodoDbDto>(
                    "spExc_Periodo_Meses",
                    new
                    {
                        PeriodoId = periodoId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return resultado
                    .Select(x => new DropDownListaGenericaModel
                    {
                        item = x.IdX,
                        descripcion = x.ItmX
                    })
                    .ToList();
            });
        }

        private sealed class CortePeriodoDbDto
        {
            public string IdX { get; set; } = string.Empty;
            public string ItmX { get; set; } = string.Empty;
        }

        /// <summary>
        /// Obtiene los tipos de distribución habilitados para el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Distribucion_Tipos_Lista_Obtener(int CodEmpresa,string usuario)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -2,
                    Description = "Debe indicar el usuario.",
                    Result = []
                };
            }

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = """
                    SELECT
                        'C' AS item,
                        'Cargado [%]' AS descripcion
                    FROM EXC_APLICADORES A
                    INNER JOIN USUARIOS U
                        ON A.USUARIO = U.NOMBRE
                    WHERE U.ESTADO = 'A'
                      AND A.ACTIVO = 1
                      AND A.CARGA = 1
                      AND U.NOMBRE = @Usuario

                    UNION ALL

                    SELECT
                        'R' AS item,
                        'Real Contable' AS descripcion
                    FROM EXC_APLICADORES A
                    INNER JOIN USUARIOS U
                        ON A.USUARIO = U.NOMBRE
                    WHERE U.ESTADO = 'A'
                      AND A.ACTIVO = 1
                      AND A.REAL = 1
                      AND U.NOMBRE = @Usuario

                    UNION ALL

                    SELECT
                        'P' AS item,
                        'Proyectado' AS descripcion
                    FROM EXC_APLICADORES A
                    INNER JOIN USUARIOS U
                        ON A.USUARIO = U.NOMBRE
                    WHERE U.ESTADO = 'A'
                      AND A.ACTIVO = 1
                      AND A.PROYECTADO = 1
                      AND U.NOMBRE = @Usuario

                    UNION ALL

                    SELECT
                        'T' AS item,
                        'Prorrateado' AS descripcion
                    FROM EXC_APLICADORES A
                    INNER JOIN USUARIOS U
                        ON A.USUARIO = U.NOMBRE
                    WHERE U.ESTADO = 'A'
                      AND A.ACTIVO = 1
                      AND A.PRORRATEADO = 1
                      AND U.NOMBRE = @Usuario;
                    """;

                return conn
                    .Query<DropDownListaGenericaModel>(
                        sql,
                        new
                        {
                            Usuario = usuario
                        })
                    .ToList();
            });
        }

        /// <summary>
        /// Obtiene el detalle integral de los montos a distribuir
        /// para el período seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <returns></returns>
        public ErrorDto<List<AHExcMontoListadoDto>>AH_Excedentes_Distribucion_Montos_Lista_Obtener(int CodEmpresa,string periodo)
        {
            periodo = (periodo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(periodo))
            {
                return new ErrorDto<List<AHExcMontoListadoDto>>
                {
                    Code = -2,
                    Description = "Debe indicar el período.",
                    Result = []
                };
            }

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                return conn
                    .Query<AHExcMontoListadoDto>(
                        "spExc_Mnt_Distribuir",
                        new
                        {
                            PeriodoId = periodo
                        },
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            });
        }

        /// <summary>
        /// Calcula el monto de distribución según período, corte,
        /// tipo, base de cálculo y porcentaje.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <param name="corte"></param>
        /// <param name="tipo"></param>
        /// <param name="baseCalculo"></param>
        /// <param name="porcentaje"></param>
        /// <returns></returns>
        public ErrorDto<decimal>
            AH_Excedentes_Distribucion_Monto_Calculo_Obtener(int CodEmpresa,string periodo,string corte,string tipo, string baseCalculo,decimal porcentaje)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = """
                    EXEC spExc_Mnt_Distribuir_Calculo
                        @Periodo,
                        @Corte,
                        @Tipo,
                        @BaseCalculo,
                        @Porcentaje;
                    """;

                decimal? monto = conn.QueryFirstOrDefault<decimal?>(
                    sql,
                    new
                    {
                        Periodo = periodo,
                        Corte = corte,
                        Tipo = tipo,
                        BaseCalculo = baseCalculo,
                        Porcentaje = porcentaje
                    });

                return monto ?? 0m;
            });
        }

        /// <summary>
        /// Indica si el monto cargado ya fue distribuido.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <param name="corte"></param>
        /// <returns></returns>
        public ErrorDto<bool>AH_Excedentes_Distribucion_Aplicada_Obtener(int CodEmpresa,string periodo,string corte)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = """
                    SELECT ISNULL(
                        dbo.fxExc_ConsultaDistribucionAplicada(
                            @Periodo,
                            @Corte
                        ),
                        0
                    );
                    """;

                int aplicado = conn.QueryFirstOrDefault<int>(
                    sql,
                    new
                    {
                        Periodo = periodo,
                        Corte = ObtenerCorteFinDia(corte)
                    });

                return aplicado == 1;
            });
        }

        /// <summary>
        /// Guarda el monto de distribución de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Distribucion_Monto_Guardar(int CodEmpresa,string usuario,AHExcMontoDto dto)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario del sistema."
                };
            }

            if (dto is null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a guardar."
                };
            }

            dto.periodo = (dto.periodo ?? string.Empty).Trim();
            dto.corte = (dto.corte ?? string.Empty).Trim();
            dto.tipo = (dto.tipo ?? string.Empty).Trim().ToUpperInvariant();
            dto.baseCalculo =
                (dto.baseCalculo ?? string.Empty).Trim().ToUpperInvariant();
            dto.justificacion =
                (dto.justificacion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(dto.periodo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el período."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.corte))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el corte."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.tipo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el tipo de distribución."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.baseCalculo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la base de cálculo."
                };
            }

            using var conn = DbHelper.OpenConnection(
                _portalDB,
                CodEmpresa);

            try
            {
                const string sql = """
                    EXEC spExc_Montos_Distribucion_Tabla_Add
                        @Periodo,
                        @Movimiento,
                        @Usuario,
                        @Corte,
                        @Tipo,
                        @BaseCalculo,
                        @Monto,
                        @Porcentaje,
                        @Justificacion;
                    """;

                conn.Execute(
                    sql,
                    new
                    {
                        Periodo = dto.periodo,
                        Movimiento = "A",
                        Usuario = usuario,
                        Corte = ObtenerCorteFinDia(dto.corte),
                        Tipo = dto.tipo,
                        BaseCalculo = dto.baseCalculo,
                        Monto = dto.monto,
                        Porcentaje = dto.porcentaje,
                        Justificacion =
                            LimitarTexto(dto.justificacion, 200)
                    });

                return DbHelper.OkResponse(
                    "Registro guardado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un monto de distribución de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Distribucion_Monto_Eliminar(int CodEmpresa,string usuario,AHExcMontoDto dto)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario del sistema."
                };
            }

            if (dto is null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a eliminar."
                };
            }

            dto.periodo = (dto.periodo ?? string.Empty).Trim();
            dto.corte = (dto.corte ?? string.Empty).Trim();
            dto.tipo = (dto.tipo ?? string.Empty).Trim().ToUpperInvariant();
            dto.baseCalculo = (dto.baseCalculo ?? string.Empty).Trim().ToUpperInvariant();
            dto.justificacion = (dto.justificacion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(dto.periodo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el período."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.corte))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el corte."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.tipo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el tipo de distribución."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.baseCalculo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la base de cálculo."
                };
            }

            using var conn = DbHelper.OpenConnection(
                _portalDB,
                CodEmpresa);

            try
            {
                const string sql = """
            EXEC spExc_Montos_Distribucion_Tabla_Add
                @Periodo,
                @Movimiento,
                @Usuario,
                @Corte,
                @Tipo,
                @BaseCalculo,
                @Monto,
                @Porcentaje,
                @Justificacion;
            """;

                conn.Execute(
                    sql,
                    new
                    {
                        Periodo = dto.periodo,
                        Movimiento = "B",
                        Usuario = usuario,
                        Corte = ObtenerCorteFinDia(dto.corte),
                        Tipo = dto.tipo,
                        BaseCalculo = dto.baseCalculo,
                        Monto = dto.monto,
                        Porcentaje = dto.porcentaje,
                        Justificacion = LimitarTexto(dto.justificacion, 200)
                    });

                return DbHelper.OkResponse(
                    "Registro eliminado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string ObtenerCorteFinDia(string corte)
        {
            corte = (corte ?? string.Empty).Trim();

            if (!DateTime.TryParse(
                corte,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime fecha))
            {
                throw new FormatException("El formato de la fecha de corte no es válido.");
            }

            return $"{fecha:yyyy-MM-dd} 23:59";
        }

        private static string LimitarTexto(
            string texto,
            int longitudMaxima)
        {
            texto = (texto ?? string.Empty).Trim();

            return texto.Length <= longitudMaxima
                ? texto
                : texto[..longitudMaxima];
        }
    }
}