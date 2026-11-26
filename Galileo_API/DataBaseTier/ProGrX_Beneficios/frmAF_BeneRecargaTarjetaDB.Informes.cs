using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using System.IO;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneRecargaTarjetaDB
    {
        private const string CorreoTesoreria = "tesoreria@aseccss.com";

        /// <summary>
        /// Obtiene las remesas de tarjetas con datos de proveedor, con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <returns>Lista de remesas con proveedor y total.</returns>
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiRecargaTarjProveedor_ObtenerRemesas(int CodCliente, string? filtro, int? pagina, int? paginacion)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneTarjetasRemesasDataLista();

                const string sqlCount = @"SELECT COUNT(DISTINCT R.COD_REMESA_TR)
                                          FROM AFI_BENE_TARJETAS_REMESAS R
                                          INNER JOIN AFI_BENE_TARJETAS_REGALO T ON T.COD_REMESA_TR = R.COD_REMESA_TR
                                          INNER JOIN AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO
                                          INNER JOIN PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV
                                          INNER JOIN CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT DISTINCT R.*, T.COD_PRODUCTO, P.COD_PRODUCTO_INV, I.COD_PROVEEDOR,
                                            O.DESCRIPCION AS NOMBRE_PROVEEDOR
                                     FROM AFI_BENE_TARJETAS_REMESAS R
                                     INNER JOIN AFI_BENE_TARJETAS_REGALO T ON T.COD_REMESA_TR = R.COD_REMESA_TR
                                     INNER JOIN AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO
                                     INNER JOIN PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV
                                     INNER JOIN CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR
                                     WHERE (@like IS NULL OR R.cod_remesa_tr LIKE @like OR O.DESCRIPCION LIKE @like
                                            OR O.COD_PROVEEDOR LIKE @like OR R.estado LIKE @like)
                                     ORDER BY R.registro_fecha DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Beneficios = connection.Query<AfiBeneTarjetasRemesasData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene las tarjetas de regalo recargadas de una remesa.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Lista de tarjetas recargadas.</returns>
        public ErrorDto<List<AfiBeneTarjetasData>> AfiTarjetasRegaloRecargadas_Obtener(int CodCliente, int cod_remesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT T.*,
                                        (SELECT TOP 1 descripcion FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = T.COD_BENEFICIO) AS BENEFICIO_DESC,
                                        (SELECT TOP 1 nombre FROM SOCIOS WHERE CEDULA = T.CEDULA) AS NOMBRE
                                     FROM AFI_BENE_TARJETAS_REGALO T
                                     WHERE COD_REMESA_TR = @cod_remesa";
                return connection.Query<AfiBeneTarjetasData>(sql, new { cod_remesa }).ToList();
            });
        }

        /// <summary>
        /// Envía por correo la solicitud de pago de recarga de tarjetas con los archivos adjuntos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="parametros">Datos de la remesa, proveedor, cuerpo y archivos.</param>
        /// <returns>Resultado de la operación.</returns>
        public async Task<ErrorDto> AfiTarjetasRegaloRecargadas_Enviar(int CodCliente, DocArchivoBeneRecargaTarjetaDto parametros)
        {
            try
            {
                var proveedor = ObtenerNombreProveedor(CodCliente, parametros.cod_proveedor);

                if (string.IsNullOrWhiteSpace(parametros.body))
                {
                    parametros.body = "Estimado asociado, se le notifica la solicitud de pago para la recarga de tarjetas de regalo. Por favor, revise los archivos adjuntos para mas detalles.";
                }

                return await _beneCorreo.EncolarAsync(new CorreoBeneficio
                {
                    CodEmpresa = CodCliente,
                    CodPlantilla = BeneficiosPlantillas.RecargaTarjetas,
                    CodSmtp = ObtenerCategoriaCorreo(CodCliente, parametros).Trim(),
                    Para = CorreoTesoreria,
                    Usuario = parametros.usuario ?? string.Empty,
                    Datos = new Dictionary<string, string>
                    {
                        ["MENSAJE"] = parametros.body,
                        ["REMESA"] = parametros.cod_remesa_tr.ToString(),
                        ["COD_PROVEEDOR"] = parametros.cod_proveedor.ToString(),
                        ["PROVEEDOR"] = proveedor
                    },
                    Adjuntos = (parametros.archivos ?? new List<FileTarjetasDto>())
                        .Where(a => a.filecontent is { Length: > 0 })
                        .Select(a => new AdjuntoBeneficio(a.filename ?? "archivo.pdf", Convert.ToBase64String(a.filecontent!)))
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "AfiTarjetasRegaloRecargadas_Enviar - " + ex.Message };
            }
        }

        /// <summary>
        /// Obtiene el código SMTP de la categoría asociada a la remesa/proveedor.
        /// </summary>
        private string ObtenerCategoriaCorreo(int CodCliente, DocArchivoBeneRecargaTarjetaDto parametros)
        {
            const string sql = @"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                 WHERE C.COD_CATEGORIA IN (
                                    SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                    WHERE B.COD_BENEFICIO IN (
                                        SELECT COD_BENEFICIO FROM AFI_BENE_TARJETAS_REGALO T
                                        INNER JOIN AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO
                                        INNER JOIN PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV
                                        INNER JOIN CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR
                                        WHERE T.COD_REMESA_TR = @cod_remesa_tr AND O.COD_PROVEEDOR = @cod_proveedor))";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(sql, new { parametros.cod_remesa_tr, parametros.cod_proveedor }));

            return result.Result ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el nombre del proveedor.
        /// </summary>
        private string ObtenerNombreProveedor(int CodCliente, int cod_proveedor)
        {
            const string sql = "SELECT TOP 1 DESCRIPCION FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @cod_proveedor";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(sql, new { cod_proveedor }));

            return result.Result ?? string.Empty;
        }
    }
}
