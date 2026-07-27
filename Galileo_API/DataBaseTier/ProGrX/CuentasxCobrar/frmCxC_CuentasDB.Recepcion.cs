using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        #region Recepcion

        /// <summary>
        /// Obtiene la lista lazy de personas para búsqueda de cédula en CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="filtros">Filtros lazy de la búsqueda.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de personas y total de registros.</returns>
        public ErrorDto<CxCCuentasPersonasFiltroLista> CxCCuentasPersonasFiltro_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "cedula" => "Cedula",
                "nombre" => "Nombre",
                "categoria" => "Categoria",
                _ => "Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string where = @"
            WHERE
                (@filtro IS NULL)
                OR (ISNULL(Cedula, '') LIKE @like)
                OR (ISNULL(Nombre, '') LIKE @like)
                OR (ISNULL(Categoria, '') LIKE @like)";

            var sqlCount = $@"
            SELECT COUNT(1)
            FROM vCxC_Personas_Filtro
            {where};";

            var sqlLista = $@"
            SELECT
                ISNULL(Cedula, '') AS cedula,
                ISNULL(Nombre, '') AS nombre,
                ISNULL(Categoria, '') AS categoria
            FROM vCxC_Personas_Filtro
            {where}
            ORDER BY {orderByField} {direction}
            ";

            var listaResponse = EjecutarListaLazy<CxCCuentasPersonasFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    mensajeDb = "No fue posible consultar las personas de CxC.",
                    mensajeGeneral = "Error inesperado al consultar las personas de CxC."
                });

            if (listaResponse.Code == -1)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPersonasFiltroLista>(listaResponse.Description);
            }

            return DbHelper.CreateOkResponse(new CxCCuentasPersonasFiltroLista
            {
                total = listaResponse.Result?.total ?? 0,
                lista = listaResponse.Result?.lista ?? new List<CxCCuentasPersonasFiltroItem>()
            });
        }

        /// <summary>
        /// Obtiene una persona de CxC por cédula desde la vista de búsqueda.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula a consultar.</param>
        /// <returns>Registro encontrado de la vista vCxC_Personas_Filtro.</returns>
        public ErrorDto<CxCCuentasPersonasFiltroItem> CxCCuentasPersonaFiltroPorCedula_Obtener(int codEmpresa, string cedula)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPersonasFiltroItem>("La cédula es requerida.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Cedula, '') AS cedula,
                ISNULL(Nombre, '') AS nombre,
                ISNULL(Categoria, '') AS categoria
            FROM vCxC_Personas_Filtro
            WHERE Cedula = @cedula;";

            return EjecutarConsultaUnica<CxCCuentasPersonasFiltroItem>(
                codEmpresa,
                sql,
                new { cedula = cedulaNormalizada },
                "No se encontró la cédula.",
                "No fue posible consultar la cédula.",
                "Error inesperado al consultar la cédula.");
        }

        /// <summary>
        /// Obtiene un concepto de CxC por código.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <returns>Datos del concepto.</returns>
        public ErrorDto<CxCCuentasConceptoData> CxCCuentasConcepto_Obtener(int codEmpresa, string codConcepto)
        {
            var codigoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptoData>("El concepto es requerido.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(C.cod_Concepto, '') AS cod_concepto,
                ISNULL(C.Descripcion, '') AS descripcion,
                ISNULL(C.Requiere_Contrato, 0) AS requiere_contrato,
                ISNULL(C.Proceso_Descuento, 0) AS proceso_descuento,
                ISNULL(C.PAGADOR_DEFAULT, '') AS pagadorid,
                ISNULL(P.Nombre, '') AS pagadordesc,
                ISNULL(C.Genera_Desembolso, 0) AS genera_desembolso
            FROM CxC_Conceptos C
            LEFT JOIN CxC_Personas P
                ON C.PAGADOR_DEFAULT = P.cedula
            WHERE C.cod_Concepto = @codConcepto;";

            return EjecutarConsultaUnica<CxCCuentasConceptoData>(
                codEmpresa,
                sql,
                new { codConcepto = codigoNormalizado },
                "No se encontró el concepto.",
                "No fue posible consultar el concepto.",
                "Error inesperado al consultar el concepto.");
        }

        /// <summary>
        /// Obtiene el concepto anterior o siguiente para navegación.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConcepto">Código actual.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Concepto encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasConceptosFiltroItem> CxCCuentasConceptoScroll_Obtener(int codEmpresa, string codConcepto, int tipo)
        {
            var codigoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptosFiltroItem>("El concepto es requerido.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto < @codConcepto
            ORDER BY cod_Concepto DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto > @codConcepto
            ORDER BY cod_Concepto ASC;";

            return EjecutarConsultaScroll<CxCCuentasConceptosFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new { codConcepto = codigoNormalizado },
                    mensajeNoEncontrado = "No hay más conceptos para navegar.",
                    mensajeDb = "No fue posible navegar conceptos.",
                    mensajeGeneral = "Error inesperado al navegar conceptos."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de conceptos activos para búsqueda.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de conceptos.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>> CxCCuentasConceptosFiltro_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "descripcion" => "Descripcion",
                _ => "cod_Concepto"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string where = @"
            WHERE Activo = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(cod_Concepto, '') LIKE @like
                    OR ISNULL(Descripcion, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            FROM CxC_Conceptos
            {where};";

            var sqlLista = $@"
            SELECT
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            {where}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasConceptosFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    mensajeDb = "No fue posible consultar conceptos.",
                    mensajeGeneral = "Error inesperado al consultar conceptos."
                });
        }

        /// <summary>
        /// Obtiene el detalle de un contrato según la cédula y contrato seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <returns>Detalle del contrato.</returns>
        public ErrorDto<CxCCuentasContratoData> CxCCuentasContratoDetalle_Obtener(int codEmpresa, string codContrato, string cedula)
        {
            var contratoNormalizado = NormalizarTexto(codContrato);
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratoData>("El contrato es requerido.");
            }

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratoData>("La cédula es requerida.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Cnt.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cnt.Descripcion, '') AS descripcion,
                ISNULL(Cnt.PAGADORES_ABIERTO, 0) AS pagadores_abierto,
                ISNULL(Per.Tasa_Corriente, Cnt.Tasa_Corriente) AS tasa_corriente,
                ISNULL(Per.Tasa_Mora, Cnt.Tasa_Mora) AS tasa_mora,
                ISNULL(Per.Plazo, Cnt.Plazo) AS plazo
            FROM CxC_Contratos Cnt
            LEFT JOIN CxC_Personas_Contratos Per
                ON Cnt.Cod_Contrato = Per.cod_contrato
               AND Per.Activo = 1
               AND Per.Cedula = @cedula
            WHERE Cnt.cod_Contrato = @codContrato
              AND (Per.Cedula IS NOT NULL OR Cnt.Suscripcion_Abierta = 1);";

            return EjecutarConsultaUnica<CxCCuentasContratoData>(
                codEmpresa,
                sql,
                new
                {
                    codContrato = contratoNormalizado,
                    cedula = cedulaNormalizada
                },
                "No se encontró el contrato.",
                "No fue posible consultar el contrato.",
                "Error inesperado al consultar el contrato.");
        }

        /// <summary>
        /// Obtiene el contrato anterior o siguiente permitido para el cliente y concepto.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <param name="codContrato">Contrato actual.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Contrato encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasContratosFiltroItem> CxCCuentasContratoScroll_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            string codContrato,
            int tipo)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);
            var conceptoNormalizado = NormalizarTexto(codConcepto);
            var contratoNormalizado = NormalizarTexto(codContrato);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratosFiltroItem>("La cédula y el concepto son requeridos.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(Cn.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cn.Descripcion, '') AS descripcion
            FROM CxC_Conceptos_Contratos Cnt
            INNER JOIN CxC_Contratos Cn
                ON Cnt.Cod_Contrato = Cn.cod_Contrato
            LEFT JOIN CxC_Personas_Contratos Pc
                ON Cnt.cod_Contrato = Pc.cod_Contrato
               AND Cnt.Cod_Concepto = @codConcepto
               AND Pc.Cedula = @cedula
            WHERE Cn.Activo = 1
              AND Cnt.Cod_Concepto = @codConcepto
              AND (Pc.Cedula IS NOT NULL OR Cn.Suscripcion_Abierta = 1)
              AND Cn.cod_contrato < @codContrato
            ORDER BY Cn.cod_contrato DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(Cn.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cn.Descripcion, '') AS descripcion
            FROM CxC_Conceptos_Contratos Cnt
            INNER JOIN CxC_Contratos Cn
                ON Cnt.Cod_Contrato = Cn.cod_Contrato
            LEFT JOIN CxC_Personas_Contratos Pc
                ON Cnt.cod_Contrato = Pc.cod_Contrato
               AND Cnt.Cod_Concepto = @codConcepto
               AND Pc.Cedula = @cedula
            WHERE Cn.Activo = 1
              AND Cnt.Cod_Concepto = @codConcepto
              AND (Pc.Cedula IS NOT NULL OR Cn.Suscripcion_Abierta = 1)
              AND Cn.cod_contrato > @codContrato
            ORDER BY Cn.cod_contrato ASC;";

            return EjecutarConsultaScroll<CxCCuentasContratosFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new
                    {
                        cedula = cedulaNormalizada,
                        codConcepto = conceptoNormalizado,
                        codContrato = contratoNormalizado
                    },
                    mensajeNoEncontrado = "No hay más contratos para navegar.",
                    mensajeDb = "No fue posible navegar contratos.",
                    mensajeGeneral = "Error inesperado al navegar contratos."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de contratos permitidos para un cliente y concepto.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <param name="filtros">Filtros lazy de búsqueda.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de contratos.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>> CxCCuentasContratosFiltro_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var cedulaNormalizada = NormalizarTexto(cedula);
            var conceptoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>>
                {
                    Code = -1,
                    Description = "La cédula y el concepto son requeridos.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "descripcion" => "Cnt.Descripcion",
                _ => "Cnt.cod_Contrato"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas_Contratos Con
            INNER JOIN CxC_Contratos Cnt
                ON Con.Cod_Contrato = Cnt.cod_contrato
            WHERE Con.cedula = @cedula
              AND Con.cod_contrato IN (
                    SELECT cod_contrato
                    FROM CxC_Conceptos_Contratos
                    WHERE cod_concepto = @codConcepto
                  )
              AND Con.Activo = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(Cnt.cod_Contrato, '') LIKE @like
                    OR ISNULL(Cnt.Descripcion, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(Cnt.cod_Contrato, '') AS cod_contrato,
                ISNULL(Cnt.Descripcion, '') AS descripcion
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasContratosFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    parametrosAdicionales = new
                    {
                        cedula = cedulaNormalizada,
                        codConcepto = conceptoNormalizado
                    },
                    mensajeDb = "No fue posible consultar contratos.",
                    mensajeGeneral = "Error inesperado al consultar contratos."
                });
        }

        /// <summary>
        /// Obtiene un pagador permitido para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedulaPagador">Cédula del pagador.</param>
        /// <returns>Datos del pagador.</returns>
        public ErrorDto<CxCCuentasPagadorData> CxCCuentasPagador_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);
            var pagadorNormalizado = NormalizarTexto(cedulaPagador);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) ||
                string.IsNullOrWhiteSpace(contratoNormalizado) ||
                string.IsNullOrWhiteSpace(pagadorNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadorData>("Cliente, contrato y pagador son requeridos.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(P.cedula, '') AS cedula,
                ISNULL(P.nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador = @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1;";

            return EjecutarConsultaUnica<CxCCuentasPagadorData>(
                codEmpresa,
                sql,
                new
                {
                    cedulaCliente = clienteNormalizado,
                    codContrato = contratoNormalizado,
                    cedulaPagador = pagadorNormalizado
                },
                "No se encontró el pagador.",
                "No fue posible consultar el pagador.",
                "Error inesperado al consultar el pagador.");
        }

        /// <summary>
        /// Obtiene el pagador anterior o siguiente permitido para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedulaPagador">Cédula actual del pagador.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Pagador encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasPagadoresFiltroItem> CxCCuentasPagadorScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador,
            int tipo)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);
            var pagadorNormalizado = NormalizarTexto(cedulaPagador);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadoresFiltroItem>("Cliente y contrato son requeridos.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador < @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1
            ORDER BY Pg.Cedula_Pagador DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador > @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1
            ORDER BY Pg.Cedula_Pagador ASC;";

            return EjecutarConsultaScroll<CxCCuentasPagadoresFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new
                    {
                        cedulaCliente = clienteNormalizado,
                        codContrato = contratoNormalizado,
                        cedulaPagador = pagadorNormalizado
                    },
                    mensajeNoEncontrado = "No hay más pagadores para navegar.",
                    mensajeDb = "No fue posible navegar pagadores.",
                    mensajeGeneral = "Error inesperado al navegar pagadores."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de pagadores permitidos para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de pagadores.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>> CxCCuentasPagadoresFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>>
                {
                    Code = -1,
                    Description = "Cliente y contrato son requeridos.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "nombre" => "P.Nombre",
                _ => "P.Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND ISNULL(Pg.Activo, 1) = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(P.Cedula, '') LIKE @like
                    OR ISNULL(P.Nombre, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasPagadoresFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    parametrosAdicionales = new
                    {
                        cedulaCliente = clienteNormalizado,
                        codContrato = contratoNormalizado
                    },
                    mensajeDb = "No fue posible consultar pagadores.",
                    mensajeGeneral = "Error inesperado al consultar pagadores."
                });
        }

        /// <summary>
        /// Obtiene un autorizado permitido para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="cedulaAutorizado">Cédula del autorizado.</param>
        /// <returns>Datos del autorizado.</returns>
        public ErrorDto<CxCCuentasAutorizadoData> CxCCuentasAutorizado_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var autorizadoNormalizado = NormalizarTexto(cedulaAutorizado);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(autorizadoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadoData>("Cliente y autorizado son requeridos.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado = @cedulaAutorizado;";

            return EjecutarConsultaUnica<CxCCuentasAutorizadoData>(
                codEmpresa,
                sql,
                new
                {
                    cedulaCliente = clienteNormalizado,
                    cedulaAutorizado = autorizadoNormalizado
                },
                "No se encontró el autorizado.",
                "No fue posible consultar el autorizado.",
                "Error inesperado al consultar el autorizado.");
        }

        /// <summary>
        /// Obtiene el autorizado anterior o siguiente permitido para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="cedulaAutorizado">Cédula actual del autorizado.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Autorizado encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasAutorizadosFiltroItem> CxCCuentasAutorizadoScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado,
            int tipo)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var autorizadoNormalizado = NormalizarTexto(cedulaAutorizado);

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadosFiltroItem>("El cliente es requerido.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado < @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado > @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado ASC;";

            return EjecutarConsultaScroll<CxCCuentasAutorizadosFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new
                    {
                        cedulaCliente = clienteNormalizado,
                        cedulaAutorizado = autorizadoNormalizado
                    },
                    mensajeNoEncontrado = "No hay más autorizados para navegar.",
                    mensajeDb = "No fue posible navegar autorizados.",
                    mensajeGeneral = "Error inesperado al navegar autorizados."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de autorizados permitidos para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de autorizados.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>> CxCCuentasAutorizadosFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var clienteNormalizado = NormalizarTexto(cedulaCliente);

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>>
                {
                    Code = -1,
                    Description = "El cliente es requerido.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "nombre" => "Per.Nombre",
                _ => "Per.Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND (
                    @filtro IS NULL
                    OR ISNULL(Per.Cedula, '') LIKE @like
                    OR ISNULL(Per.Nombre, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasAutorizadosFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    mensajeDb = "No fue posible consultar autorizados.",
                    mensajeGeneral = "Error inesperado al consultar autorizados.",
                    parametrosAdicionales = new
                    {
                        cedulaCliente = clienteNormalizado
                    }
                });
        }

        /// <summary>
        /// Obtiene las cuentas bancarias de un cliente según el banco seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="banco">Banco seleccionado.</param>
        /// <returns>Lista de cuentas bancarias.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentasCuentasBancarias_Obtener(int codEmpresa, string cedula, string banco)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);
            var bancoNormalizado = NormalizarTexto(banco);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(bancoNormalizado))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("La cédula y el banco son requeridos.");
            }

            const string sql = @"exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, 1;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    Identificacion = cedulaNormalizada,
                    BancoId = bancoNormalizado
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>($"Error inesperado al consultar las cuentas bancarias. {ex.Message}");
            }
        }

        #endregion
    }
}
