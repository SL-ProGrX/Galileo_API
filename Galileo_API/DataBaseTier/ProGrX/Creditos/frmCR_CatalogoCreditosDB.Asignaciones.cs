using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

        /// <summary>
        /// Obtiene las asignaciones disponibles de la linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCreditoAsignacionesData> CrCatalogoCreditos_Asignaciones_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoAsignacionesData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string destinosQuery = @"
                SELECT
                    R.cod_destino AS destino,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Catalogo_Destinos R
                LEFT JOIN catalogo_destinosAsg A
                    ON R.cod_destino = A.cod_destino
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_destino;";

            const string cargosQuery = @"
                SELECT
                    R.cod_cargo AS cargo,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CASE WHEN R.tipo = 'P' THEN 'Porcentual' ELSE 'Monto' END AS tipo,
                    ISNULL(R.valor, 0) AS valor,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Cargos_Adicionales R
                LEFT JOIN Cargos_asignacion A
                    ON R.cod_cargo = A.cod_cargo
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_cargo;";

            const string requisitosQuery = @"
                SELECT
                    R.cod_requisito AS requisito,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, ISNULL(A.opcional, 0)) AS opcional,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Requisitos_Adicionales R
                LEFT JOIN Requisitos_asignacion A
                    ON R.cod_requisito = A.cod_requisito
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_requisito;";

            const string recursosQuery = @"
                SELECT
                    G.cod_grupo AS recurso,
                    ISNULL(G.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM catalogo_grupos G
                LEFT JOIN catalogo_asignaGrp A
                    ON G.cod_grupo = A.cod_grupo
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, G.cod_grupo;";

            const string carteraQuery = @"
                SELECT
                    R.cod_clasificacion AS cartera,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM CBR_CLASIFICACION_CARTERA R
                LEFT JOIN CBR_CLASIFICACION_DETALLE A
                    ON R.cod_clasificacion = A.cod_clasificacion
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_clasificacion;";

            const string refundiblesQuery = @"
                SELECT
                    R.codigo,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS refunde
                FROM CATALOGO R
                LEFT JOIN CRD_CATALOGO_REFUNDIBLES A
                    ON R.codigo = A.cod_refundible
                    AND A.codigo = @Codigo
                ORDER BY refunde DESC, R.codigo;";

            var parametros = new { Codigo = codigo };
            var destinos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionDestinoData>(_portalDb, codEmpresa, destinosQuery, parametros);
            if (destinos.Code < 0) return ErrorAsignaciones(destinos.Description);

            var cargos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionCargoData>(_portalDb, codEmpresa, cargosQuery, parametros);
            if (cargos.Code < 0) return ErrorAsignaciones(cargos.Description);

            var requisitos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRequisitoData>(_portalDb, codEmpresa, requisitosQuery, parametros);
            if (requisitos.Code < 0) return ErrorAsignaciones(requisitos.Description);

            var recursos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRecursoData>(_portalDb, codEmpresa, recursosQuery, parametros);
            if (recursos.Code < 0) return ErrorAsignaciones(recursos.Description);

            var cartera = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionCarteraData>(_portalDb, codEmpresa, carteraQuery, parametros);
            if (cartera.Code < 0) return ErrorAsignaciones(cartera.Description);

            var refundibles = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRefundibleData>(_portalDb, codEmpresa, refundiblesQuery, parametros);
            if (refundibles.Code < 0) return ErrorAsignaciones(refundibles.Description);

            return new ErrorDto<CrCatalogoCreditoAsignacionesData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoAsignacionesData
                {
                    destinos = destinos.Result ?? [],
                    cargos = cargos.Result ?? [],
                    requisitos = requisitos.Result ?? [],
                    recursos = recursos.Result ?? [],
                    cartera = cartera.Result ?? [],
                    refundibles = refundibles.Result ?? []
                }
            };
        }


        /// <summary>
        /// Obtiene la lista de adjuntos disponibles para solicitudes en linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoAdjuntoData>> CrCatalogoCreditos_Adjuntos_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<CrCatalogoCreditoAdjuntoData>>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito.",
                    Result = []
                };
            }

            const string query = @"
                SELECT
                    R.COD_ADJUNTO AS id,
                    ISNULL(R.DESCRIPCION, '') AS descripcion,
                    CONVERT(bit, ISNULL(A.opcional, 0)) AS opcional,
                    CONVERT(bit, CASE WHEN A.COD_ADJUNTO IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM CRD_ADJUNTOS_TIPOS R
                LEFT JOIN CRD_CATALOGO_ADJUNTOS A
                    ON R.COD_ADJUNTO = A.COD_ADJUNTO
                    AND A.CODIGO = @Codigo
                ORDER BY asignado DESC, R.COD_ADJUNTO;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoAdjuntoData>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo });
        }


        /// <summary>
        /// Guarda una asignacion de destinos, cargos, requisitos, recursos, cartera o refundibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_Asignacion_Guardar(int codEmpresa, CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            NormalizarAsignacionRequest(request);

            if (string.IsNullOrWhiteSpace(request.codigo) || string.IsNullOrWhiteSpace(request.codigo_asignacion))
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el codigo de asignacion." };
            }

            var query = request.tipo switch
            {
                "destinos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM catalogo_DestinosAsg WHERE codigo = @Codigo AND cod_destino = @CodigoAsignacion)
                        INSERT catalogo_DestinosAsg(codigo, cod_destino) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE catalogo_DestinosAsg WHERE codigo = @Codigo AND cod_destino = @CodigoAsignacion;",
                "cargos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM cargos_asignacion WHERE codigo = @Codigo AND cod_cargo = @CodigoAsignacion)
                        INSERT cargos_asignacion(codigo, cod_cargo) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE cargos_asignacion WHERE codigo = @Codigo AND cod_cargo = @CodigoAsignacion;",
                "requisitos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM requisitos_asignacion WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion)
                            INSERT requisitos_asignacion(codigo, cod_requisito, opcional) VALUES(@Codigo, @CodigoAsignacion, @Opcional);
                        ELSE
                            UPDATE requisitos_asignacion SET opcional = @Opcional WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion;"
                    : @"DELETE requisitos_asignacion WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion;",
                "recursos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM catalogo_asignaGrp WHERE codigo = @Codigo AND cod_grupo = @CodigoAsignacion)
                        INSERT catalogo_asignaGrp(codigo, cod_grupo) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE catalogo_asignaGrp WHERE codigo = @Codigo AND cod_grupo = @CodigoAsignacion;",
                "cartera" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CBR_CLASIFICACION_DETALLE WHERE codigo = @Codigo AND cod_clasificacion = @CodigoAsignacion)
                        INSERT CBR_CLASIFICACION_DETALLE(codigo, cod_clasificacion) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE CBR_CLASIFICACION_DETALLE WHERE codigo = @Codigo AND cod_clasificacion = @CodigoAsignacion;",
                "refundibles" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CATALOGO_REFUNDIBLES WHERE codigo = @Codigo AND cod_refundible = @CodigoAsignacion)
                        INSERT CRD_CATALOGO_REFUNDIBLES(codigo, cod_refundible, registro_fecha, registro_usuario)
                        VALUES(@Codigo, @CodigoAsignacion, dbo.mygetdate(), @Usuario);"
                    : @"DELETE CRD_CATALOGO_REFUNDIBLES WHERE codigo = @Codigo AND cod_refundible = @CodigoAsignacion;",
                "adjuntos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CATALOGO_ADJUNTOS WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion)
                            INSERT CRD_CATALOGO_ADJUNTOS(codigo, COD_ADJUNTO, opcional, REGISTRO_USUARIO, REGISTRO_FECHA)
                            VALUES(@Codigo, @CodigoAsignacion, @Opcional, @Usuario, dbo.mygetdate());
                        ELSE
                            UPDATE CRD_CATALOGO_ADJUNTOS SET opcional = @Opcional WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion;"
                    : @"DELETE CRD_CATALOGO_ADJUNTOS WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion;",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto { Code = -1, Description = "Tipo de asignacion invalido." };
            }

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    CodigoAsignacion = request.codigo_asignacion,
                    Opcional = request.opcional ? 1 : 0,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.asignado ? "Registra - WEB" : "Borrar - WEB",
                    $"Catalogo Creditos > {request.tipo}: {request.codigo_asignacion} a la Linea: {request.codigo}");
            }

            return respuesta;
        }
    }
}
