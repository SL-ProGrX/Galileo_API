using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioAsgDB
    {
        /// <summary>
        /// Lista paginada de beneficios otorgados al socio, con filtro opcional.
        /// </summary>
        public ErrorDto<AfiBeneOtorgaAsgDataList> AfiBeneOtorga_Obtener(int CodCliente, string cedula, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var filtroLike = string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro.Trim()}%";
                var aplicarPaginacion = pagina.HasValue
                    && paginacion.HasValue
                    && paginacion.Value > 0;

                var p = new DynamicParameters();
                p.Add("@cedula", cedula, DbType.String);
                p.Add("@filtroLike", filtroLike, DbType.String);
                p.Add("@offset", aplicarPaginacion ? pagina!.Value : 0, DbType.Int32);
                p.Add("@fetch", aplicarPaginacion ? paginacion!.Value : int.MaxValue, DbType.Int32);

                var datos = new AfiBeneOtorgaAsgDataList
                {
                    total = connection.QueryFirstOrDefault<int>(
                        "SELECT COUNT(*) FROM afi_bene_otorga WHERE cedula = @cedula", p)
                };

                const string sql = @"SELECT O.*, B.Descripcion FROM afi_bene_otorga O
                                     INNER JOIN afi_beneficios B ON O.cod_beneficio = B.cod_beneficio
                                     WHERE O.cedula = @cedula
                                       AND (@filtroLike IS NULL
                                            OR B.Descripcion LIKE @filtroLike
                                            OR O.cod_beneficio LIKE @filtroLike
                                            OR O.consec LIKE @filtroLike)
                                     ORDER BY O.cod_beneficio
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                datos.beneficios = connection.Query<AfiBeneOtorgaData>(sql, p).ToList();
                return datos;
            });

            return new ErrorDto<AfiBeneOtorgaAsgDataList>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new AfiBeneOtorgaAsgDataList()
            };
        }

        /// <summary>Detalle del beneficio (catálogo).</summary>
        public ErrorDto<List<AfiBeneDto>> BeneficioDetalle_Obtener(int CodCliente, string cod_beneficio)
        {
            const string sql = @"SELECT RTRIM(cod_Beneficio) AS cod_Beneficio, RTRIM(descripcion) AS descripcion, tipo, monto,
                                        modifica_diferencia, aplica_beneficiarios, aplica_parcial
                                 FROM afi_beneficios WHERE cod_beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneDto>(sql, new { codBeneficio = cod_beneficio }).ToList());

            return new ErrorDto<List<AfiBeneDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneDto>()
            };
        }

        /// <summary>Beneficio otorgado a un socio por código y consecutivo.</summary>
        public ErrorDto<List<AfiBeneOtorgaData>> AfiBeneOtorgaSocio_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            const string sql = @"SELECT A.*, O.descripcion, S.Nombre AS sNombre
                                 FROM afi_bene_otorga A
                                 INNER JOIN Socios S ON A.cedula = S.cedula
                                 LEFT JOIN Sif_Oficinas O ON A.cod_oficina = O.cod_Oficina
                                 WHERE cod_beneficio = @codBeneficio AND consec = @consec";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneOtorgaData>(sql, new { codBeneficio, consec }).ToList());

            return new ErrorDto<List<AfiBeneOtorgaData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneOtorgaData>()
            };
        }

        /// <summary>Pagos (órdenes) de un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioPagos_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            const string sql = @"SELECT Bp.*, B.Descripcion AS 'BancoDesc'
                                 FROM afi_bene_pago Bp
                                 LEFT JOIN Tes_Bancos B ON Bp.cod_Banco = B.id_Banco
                                 WHERE Bp.consec = @consec AND Bp.cod_beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneficioPago>(sql, new { consec, codBeneficio }).ToList());

            return new ErrorDto<List<AfiBeneficioPago>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneficioPago>()
            };
        }

        /// <summary>Nombre del beneficiario asociado.</summary>
        public ErrorDto Beneficiario_Obtener(int CodCliente, string cedulabn, string cedula)
        {
            const string sql = "SELECT Nombre FROM beneficiarios WHERE cedulabn = @cedulabn AND cedula = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(sql, new { cedulabn, cedula }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? (result.Result ?? string.Empty) : result.Description
            };
        }

        /// <summary>Productos asignados a un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioProducto_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            const string sql = @"SELECT R.*, P.Descripcion AS 'ProdDesc', P.costo_unidad AS 'ProdCu'
                                 FROM afi_bene_prodasg R
                                 INNER JOIN afi_bene_productos P ON R.cod_Producto = P.cod_Producto
                                 WHERE R.consec = @consec AND R.cod_beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneficioPago>(sql, new { consec, codBeneficio = codBeneficio.Trim() }).ToList());

            return new ErrorDto<List<AfiBeneficioPago>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneficioPago>()
            };
        }

        /// <summary>Lista de tipos de beneficio disponibles para el usuario.</summary>
        public ErrorDto<List<BeneficioData>> BeneficioUsuario_Obtener(int CodCliente, string usuario)
        {
            const string sql = @"SELECT RTRIM(cod_Beneficio) AS cod_beneficio, RTRIM(descripcion) AS descripcion
                                 FROM afi_beneficios
                                 WHERE estado = 'A' AND cod_beneficio IN (
                                     SELECT cod_beneficio FROM AFI_BENE_GRUPOSB WHERE cod_grupo IN (
                                         SELECT cod_grupo FROM AFI_BENE_USERG WHERE usuario = @usuario))";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<BeneficioData>(sql, new { usuario }).ToList());

            return new ErrorDto<List<BeneficioData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<BeneficioData>()
            };
        }

        /// <summary>
        /// Obtiene los datos del asiento contable (cuenta bancaria y del beneficio) y el monto de la boleta.
        /// </summary>
        public ErrorDto<AsientoContableData> AsientoContableData_Obtener(int CodCliente, string cod_beneficio, string cedula, int consec)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var datos = new AsientoContableData();

                var bancos = connection.QueryFirstOrDefault<int>(
                    @"SELECT cod_banco FROM afi_bene_pago WHERE cedula = @cedula AND cod_beneficio = @codBeneficio AND consec = @consec",
                    new { cedula, codBeneficio = cod_beneficio, consec });

                var ctaContable = connection.QueryFirstOrDefault<CuentasBancariasModels>(
                    "SELECT ctaconta AS cuenta, descripcion FROM Tes_Bancos WHERE id_banco = @bancos", new { bancos });

                var ctaBeneficio = connection.QueryFirstOrDefault<CuentasBancariasModels>(
                    "SELECT cod_cuenta AS cuenta, descripcion FROM afi_beneficios WHERE cod_beneficio = @codBeneficio",
                    new { codBeneficio = cod_beneficio });

                var monto = connection.QueryFirstOrDefault<float>(
                    "SELECT monto FROM afi_bene_otorga WHERE consec = @consec AND cedula = @cedula AND cod_beneficio = @codBeneficio",
                    new { consec, cedula, codBeneficio = cod_beneficio });

                datos.fxcuentabanco = ctaContable?.cuenta ?? "ND";
                datos.fxDescripcion = ctaContable?.descripcion ?? "ND";
                datos.fxDescribe = ctaBeneficio?.descripcion ?? "ND";
                datos.fxcuenta = ctaBeneficio?.cuenta ?? "ND";
                datos.fxmonto = monto;
                datos.fxmontobene = monto;

                return datos;
            });

            return new ErrorDto<AsientoContableData>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new AsientoContableData()
            };
        }

        /// <summary>
        /// Obtiene el detalle completo del beneficio (catálogo AFI_BENEFICIOS).
        /// Usado por otros procesos (por ejemplo, el guardado general de Beneficios Integrales).
        /// </summary>
        public ErrorDto<AfiBeneficiosDto> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
        {
            const string sql = "SELECT * FROM afi_beneficios WHERE Cod_Beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneficiosDto>(sql, new { codBeneficio = Cod_Beneficio }));

            return new ErrorDto<AfiBeneficiosDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Carga la lista de oficinas del usuario (SP sbSIFOficinasUsuario).
        /// </summary>
        public ErrorDto<List<SifOficinasUsuarioResultDto>> CargaOficinas(int CodCliente, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<SifOficinasUsuarioResultDto>(
                    "[sbSIFOficinasUsuario]", new { Usuario = usuario }, commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<SifOficinasUsuarioResultDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<SifOficinasUsuarioResultDto>()
            };
        }
    }
}
