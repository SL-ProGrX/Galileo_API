using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrCatalogoPolizasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCrCatalogoPolizasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }


        private void Bitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = 3
            });
        }

        #region Definicion

        /// <summary>
        /// Llena combo de Aplicación (VB6: cboAplicacion) desde POLIZAS_GRUPO.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_GrupoAplicacion_Listar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            ID_POLIZA_GRUPO AS item,
                            RTRIM(descripcion) AS descripcion
                        FROM POLIZAS_GRUPO
                        WHERE activo = 1
                        ORDER BY ID_POLIZA_GRUPO";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Lista de aseguradoras activas para cboAseguradora (VB6: CRD_POLIZAS_ASEGURADORAS where activo = 1).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_Aseguradoras_Listar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        COD_ASEGURADORA AS item,
                        RTRIM(NOMBRE) AS descripcion
                    FROM CRD_POLIZAS_ASEGURADORAS
                    WHERE activo = 1
                    ORDER BY NOMBRE";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Lista todas las pólizas para el grid lswPolizas (VB6: sbPolizaLista).
        /// </summary>
        public ErrorDto<List<CrdCatalogoPolizasListDto>> Crd_CatalogoPolizas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
               const string query = @"
                    SELECT
                        cod_poliza,
                        descripcion,
                        base       AS [base],
                        tipo,
                        valor,
                        porc_formalizacion,
                        codigo_retencion,
                        codigo_cargo,
                        cod_cuenta
                    FROM CRD_CATALOGO_POLIZAS
                    ORDER BY cod_poliza";

                return conn.Query<CrdCatalogoPolizasListDto>(query).ToList();
            });
        }

        /// <summary>
        /// Muestra los detalles de una póliza específica para el formulario de edición (VB6: sbPolizaConsulta).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_poliza"></param>
        /// <returns></returns>
        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPoliza_Obtener(int CodEmpresa, string? cod_poliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spCrd_Poliza_Catalogo_Consulta @cod";
                return conn.QueryFirstOrDefault<CrdCatalogoPolizasConsultaDto>(
                    query,
                    new { cod = cod_poliza }
                );
            });
        }

        /// <summary>
        /// Método para navegar entre pólizas (siguiente/anterior) en el formulario de edición, basado en el código de póliza actual y la dirección de navegación (VB6: sbPolizaNavegar).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_poliza"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPolizas_Navegar(
                int CodEmpresa,
                string cod_poliza,
                string direccion // "N" = siguiente, "A" = anterior
            )
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                string sqlCodigo = direccion == "A"
                    ? @"SELECT TOP 1 cod_poliza 
                FROM CRD_CATALOGO_POLIZAS
                WHERE cod_poliza < @cod
                ORDER BY cod_poliza DESC"
                    : @"SELECT TOP 1 cod_poliza 
                FROM CRD_CATALOGO_POLIZAS
                WHERE cod_poliza > @cod
                ORDER BY cod_poliza ASC";

                var nuevoCodigo = conn.QueryFirstOrDefault<string>(sqlCodigo, new { cod = cod_poliza.Trim() });

                return Crd_CatalogoPoliza_Obtener(CodEmpresa, nuevoCodigo).Result;

            });
        }

        /// <summary>
        /// Ejecuta actualización masiva de pólizas.
        /// </summary>
        public ErrorDto<bool> Crd_CatalogoPolizas_ActualizarMasivo(int CodEmpresa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            try
            {
                const string query = @"EXEC spCrdPolizaActualizaCalculo_CtaSld 0";

                var result = connection.Execute(query);

                if (result >= 0)
                {
                    return DbHelper.CreateOkResponse<bool>(true);
                }

                return DbHelper.CreateOkResponse<bool>(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>("Error al actualizar las pólizas.", -1, false);
            }
        }

        /// <summary>
        /// Lista de acreedores para una póliza (lswAcreedores).
        /// </summary>
        public ErrorDto<List<CrdCatalogoPolizasAcreedorDto>> Crd_CatalogoPolizas_Acreedores_Obtener(
            int CodEmpresa,
            string? cod_poliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        Acr.COD_ACREEDOR        AS cod_acreedor,
                        Acr.IDENTIFICACION      AS identificacion,
                        Acr.NOMBRE              AS nombre,
                        Asg.registro_fecha,
                        Asg.registro_usuario
                    FROM CRD_POLIZAS_ACREEDORES Acr
                    LEFT JOIN CRD_POLIZAS_ACREEDOR_ASG Asg
                        ON Acr.cod_acreedor = Asg.cod_acreedor
                        AND Asg.cod_poliza = @cod_poliza
                    WHERE Acr.Activo = 1
                    ORDER BY Asg.registro_fecha DESC, Acr.COD_ACREEDOR";

                var lista = conn.Query<CrdCatalogoPolizasAcreedorDto>(
                    query,
                    new { cod_poliza }
                ).ToList();

                // Igual que VB6: si registro_fecha != null → checked
                foreach (var item in lista)
                {
                    item.asignado = item.registro_fecha != null;
                }

                return lista;
            });
        }


        public ErrorDto<bool> Crd_CatalogoPolizas_Acreedor_Asignar(int CodEmpresa, string usuario, CrdCatalogoPolizasAcreedorAsignarReq req)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var response = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = false };

            try
            {
                if (req.asignar)
                {
                    const string sql = @"
                            INSERT CRD_POLIZAS_ACREEDOR_ASG(cod_poliza,cod_acreedor,registro_fecha,registro_usuario)
                            VALUES(@cod_poliza,@cod_acreedor,dbo.MyGetdate(),@usuario)";
                    var rows = connection.Execute(sql, new { cod_poliza = req.cod_poliza.Trim(), cod_acreedor = req.cod_acreedor.Trim(), usuario });
                    response.Result = rows > 0;
                }
                else
                {
                    const string sql = @"
                            DELETE CRD_POLIZAS_ACREEDOR_ASG
                            WHERE cod_poliza = @cod_poliza AND cod_acreedor = @cod_acreedor";
                    var rows = connection.Execute(sql, new { cod_poliza = req.cod_poliza.Trim(), cod_acreedor = req.cod_acreedor.Trim() });
                    response.Result = rows > 0;
                }

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>($"Error asignando acreedor: {ex.Message}", -1, false);
            }
        }


        /// <summary>
        /// Lista las garantías asociadas a una póliza.
        /// (VB6: sbPolizaGarantias)
        /// </summary>
        public ErrorDto<List<CrdCatalogoPolizasGarantiaDto>>
        Crd_CatalogoPolizas_Garantias_Listar(int CodEmpresa, string? cod_poliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spCrd_Poliza_Catalogo_Garantias @cod_poliza";

                 var response = conn.Query<CrdCatalogoPolizasGarantiaDto>(
                    query,
                    new { cod_poliza = cod_poliza }
                ).ToList();

                return response;
            });
        }

        /// <summary>
        /// Asigna o elimina una garantía de una póliza.
        /// (VB6: spCrd_Poliza_Catalogo_Garantias_Asigna)
        /// </summary>
        public ErrorDto<CrdCatalogoPolizasGarantiaAsignaDto?>
        Crd_CatalogoPolizas_Garantia_Asignar(
            int CodEmpresa,
            string usuario,
            CrdCatalogoPolizasGarantiaAsignarReq req)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
        EXEC spCrd_Poliza_Catalogo_Garantias_Asigna 
            @cod_poliza,
            @garantia,
            @accion,
            @usuario";

                return conn.QueryFirstOrDefault<CrdCatalogoPolizasGarantiaAsignaDto>(
                    query,
                    new
                    {
                        cod_poliza = req.cod_poliza.Trim(),
                        garantia = req.garantia.Trim(),
                        accion = req.asignar ? "A" : "E",
                        usuario
                    }
                );
            });
        }


        public ErrorDto Crd_CatalogoPolizas_Guardar(int CodEmpresa, string usuario, CrdCatalogoPolizasGuardarDto dto)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            // Validaciones mínimas (equivalente a lo crítico del VB)
            var codPoliza = (dto.cod_poliza ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codPoliza))
                return DbHelper.ErrorResponse("Debe indicar el código de la póliza.");

            if (string.IsNullOrWhiteSpace(dto.descripcion))
                return DbHelper.ErrorResponse("Debe indicar la descripción de la póliza.");

            if (dto.vence_dia is null || dto.vence_dia < 1 || dto.vence_dia > 30)
                return DbHelper.ErrorResponse("El día de vencimiento debe estar entre 1 y 30.");

            // Defaults como en VB (si vienen vacíos)
            var baseCod = (dto.@base ?? "C").Trim();
            var tipoCod = (dto.tipo ?? "P").Trim();

            var existe = conn.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM CRD_CATALOGO_POLIZAS WHERE cod_poliza = @cod_poliza",
                new { cod_poliza = codPoliza }
            ) > 0;

            if (!existe)
            {
                const string insertSql = @"
                            INSERT INTO CRD_CATALOGO_POLIZAS
                            (
                              cod_poliza, descripcion, base, tipo, valor, porc_formalizacion, plazo_meses, cod_cuenta,
                              codigo_retencion, codigo_cargo, cobertura_inicio, cobertura_corte, cod_aseguradora, contrato_num,
                              cobertura_vencimiento, vence_frecuencia, vence_dia, poliza_general, cobertura_region,
                              integra_plan_pagos, poliza_general_tipo, poliza_general_monto, iva_aplica, iva_incluido,
                              iva_porcentaje, id_poliza_grupo, cod_cuenta_gasto, cod_unidad, cod_centro_costo,
                              registro_fecha, registro_usuario
                            )
                            VALUES
                            (
                              @cod_poliza, @descripcion, @base, @tipo, @valor, @porc_formalizacion, @plazo_meses, @cod_cuenta,
                              @codigo_retencion, @codigo_cargo, @cobertura_inicio, @cobertura_corte, @cod_aseguradora, @contrato_num,
                              @cobertura_vencimiento, @vence_frecuencia, @vence_dia, @poliza_general, @cobertura_region,
                              @integra_plan_pagos, @poliza_general_tipo, @poliza_general_monto, @iva_aplica, @iva_incluido,
                              @iva_porcentaje, @id_poliza_grupo, @cod_cuenta_gasto, @cod_unidad, @cod_centro_costo,
                              GETDATE(), @registro_usuario
                            );";

                var rows = conn.Execute(insertSql, new
                {
                    cod_poliza = codPoliza,
                    descripcion = (dto.descripcion ?? "").Trim(),
                    @base = baseCod,
                    @tipo = tipoCod,
                    valor = dto.valor ?? 0m,
                    porc_formalizacion = dto.porc_formalizacion ?? 0m,
                    plazo_meses = dto.plazo_meses ?? 0,
                    cod_cuenta = (dto.cod_cuenta ?? "").Trim(),

                    codigo_retencion = (dto.codigo_retencion ?? "").Trim(),
                    codigo_cargo = (dto.codigo_cargo ?? "").Trim(),

                    cobertura_inicio = dto.cobertura_inicio ?? 0m,
                    cobertura_corte = dto.cobertura_corte ?? 0m,

                    cod_aseguradora = string.IsNullOrWhiteSpace(dto.cod_aseguradora) ? null : dto.cod_aseguradora.Trim(),
                    contrato_num = (dto.contrato_num ?? "").Trim(),

                    cobertura_vencimiento = dto.cobertura_vencimiento ?? DateTime.Now,
                    vence_frecuencia = (dto.vence_frecuencia ?? "").Trim(),
                    vence_dia = dto.vence_dia,

                    poliza_general = dto.poliza_general ?? 0,
                    cobertura_region = dto.cobertura_region ?? 0,
                    integra_plan_pagos = dto.integra_plan_pagos ?? 0,

                    poliza_general_tipo = (dto.poliza_general_tipo ?? "C").Trim(),
                    poliza_general_monto = dto.poliza_general_monto ?? 0m,

                    iva_aplica = dto.iva_aplica ?? 0,
                    iva_incluido = dto.iva_incluido ?? 0,
                    iva_porcentaje = dto.iva_porcentaje ?? 0m,

                    id_poliza_grupo = dto.id_poliza_grupo, // null permitido

                    cod_cuenta_gasto = (dto.cod_cuenta_gasto ?? "").Trim(),
                    cod_unidad = (dto.cod_unidad ?? "").Trim(),
                    cod_centro_costo = (dto.cod_centro_costo ?? "").Trim(),

                    registro_usuario = usuario
                });

                var result = rows > 0;
                return result ? DbHelper.OkResponse("Póliza creada correctamente.") : DbHelper.ErrorResponse("No se pudo crear la póliza.");
            }
            else
            {
                const string insertSql = @"
                        INSERT INTO CRD_CATALOGO_POLIZAS
                        (
                          cod_poliza, descripcion, base, tipo, valor, porc_formalizacion, plazo_meses, cod_cuenta,
                          codigo_retencion, codigo_cargo, cobertura_inicio, cobertura_corte, cod_aseguradora, contrato_num,
                          cobertura_vencimiento, vence_frecuencia, vence_dia, poliza_general, cobertura_region,
                          integra_plan_pagos, poliza_general_tipo, poliza_general_monto, iva_aplica, iva_incluido,
                          iva_porcentaje, id_poliza_grupo, cod_cuenta_gasto, cod_unidad, cod_centro_costo,
                          registro_fecha, registro_usuario
                        )
                        VALUES
                        (
                          @cod_poliza, @descripcion, @base, @tipo, @valor, @porc_formalizacion, @plazo_meses, @cod_cuenta,
                          @codigo_retencion, @codigo_cargo, @cobertura_inicio, @cobertura_corte, @cod_aseguradora, @contrato_num,
                          @cobertura_vencimiento, @vence_frecuencia, @vence_dia, @poliza_general, @cobertura_region,
                          @integra_plan_pagos, @poliza_general_tipo, @poliza_general_monto, @iva_aplica, @iva_incluido,
                          @iva_porcentaje, @id_poliza_grupo, @cod_cuenta_gasto, @cod_unidad, @cod_centro_costo,
                          GETDATE(), @registro_usuario
                        );";

                var rows = conn.Execute(insertSql, new
                {
                    cod_poliza = codPoliza,
                    descripcion = (dto.descripcion ?? "").Trim(),
                    @base = baseCod,
                    @tipo = tipoCod,
                    valor = dto.valor ?? 0m,
                    porc_formalizacion = dto.porc_formalizacion ?? 0m,
                    plazo_meses = dto.plazo_meses ?? 0,
                    cod_cuenta = (dto.cod_cuenta ?? "").Trim(),

                    codigo_retencion = (dto.codigo_retencion ?? "").Trim(),
                    codigo_cargo = (dto.codigo_cargo ?? "").Trim(),

                    cobertura_inicio = dto.cobertura_inicio ?? 0m,
                    cobertura_corte = dto.cobertura_corte ?? 0m,

                    cod_aseguradora = string.IsNullOrWhiteSpace(dto.cod_aseguradora) ? null : dto.cod_aseguradora.Trim(),
                    contrato_num = (dto.contrato_num ?? "").Trim(),

                    cobertura_vencimiento = dto.cobertura_vencimiento ?? DateTime.Now,
                    vence_frecuencia = (dto.vence_frecuencia ?? "").Trim(),
                    vence_dia = dto.vence_dia,

                    poliza_general = dto.poliza_general ?? 0,
                    cobertura_region = dto.cobertura_region ?? 0,
                    integra_plan_pagos = dto.integra_plan_pagos ?? 0,

                    poliza_general_tipo = (dto.poliza_general_tipo ?? "C").Trim(),
                    poliza_general_monto = dto.poliza_general_monto ?? 0m,

                    iva_aplica = dto.iva_aplica ?? 0,
                    iva_incluido = dto.iva_incluido ?? 0,
                    iva_porcentaje = dto.iva_porcentaje ?? 0m,

                    id_poliza_grupo = dto.id_poliza_grupo, // null permitido

                    cod_cuenta_gasto = (dto.cod_cuenta_gasto ?? "").Trim(),
                    cod_unidad = (dto.cod_unidad ?? "").Trim(),
                    cod_centro_costo = (dto.cod_centro_costo ?? "").Trim(),

                    registro_usuario = usuario
                });

                var result = rows > 0;
                return result ? DbHelper.OkResponse("Póliza actualizada correctamente.") : DbHelper.ErrorResponse("No se pudo actualizar la póliza.");
            }
        }


        #endregion

            #region Asignacion

            /// <summary>
            /// Método para construir el árbol de asignación de pólizas (VB6: sbCargaArbol), que muestra las líneas, destinos y garantías disponibles para asignar a una póliza. El nodo raíz es "Lineas", debajo van las líneas (L), luego los destinos (D) y finalmente las garantías (G). Cada nodo tiene un key con formato específico para identificar su tipo y código.
            /// </summary>
            /// <param name="CodEmpresa"></param>
            /// <returns></returns>
        public ErrorDto<List<CrdTreeNodeDto>> Crd_Asignacion_Arbol_Raiz(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sql = @"
                select codigo, descripcion
                from catalogo
                where retencion = 'N' and poliza = 'N' and activo = 1
                order by codigo";

                var lineas = conn.Query(sql).Select(r =>
                {
                    string codigo = (r.codigo ?? "").ToString().Trim();
                    string desc = (r.descripcion ?? "").ToString().Trim();

                    return new CrdTreeNodeDto
                    {
                        key = $"0x0{codigo}L",
                        label = $"{codigo} - {desc}",
                        leaf = false,
                        children = new List<CrdTreeNodeDto>
                        {
                            new CrdTreeNodeDto { key="__loading__", label="Cargando...", leaf=true }
                        },
                        data = new { tipo = "L", codigo = codigo }
                    };
                }).ToList();

                var root = new CrdTreeNodeDto
                {
                    key = "Lineas",
                    label = "Lineas",
                    leaf = false,
                    children = lineas,
                    data = new { tipo = "ROOT" }
                };

                return new List<CrdTreeNodeDto> { root };
            });
        }

        /// <summary>
        /// Método para cargar los nodos hijos (destinos y garantías) de un nodo de línea específico en el árbol de asignación (VB6: sbCargaArbol, expand de línea). Solo se implementa para nodos de línea (terminan en "L"). El método extrae el código de la línea del key del nodo, consulta las garantías y destinos asociados a esa línea, y construye los nodos hijos correspondientes con un formato específico en el key para identificar su tipo (D para destino, G para garantía) y su código. Si no hay destinos o garantías, el nodo se marca como hoja (leaf = true).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nodeKey"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdTreeNodeDto>> Crd_Asignacion_Arbol_Hijos(int CodEmpresa, string nodeKey)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(nodeKey))
                    return new List<CrdTreeNodeDto>();

                // Solo implementamos por ahora expand de Linea (termina en L)
                if (!nodeKey.EndsWith("L", StringComparison.OrdinalIgnoreCase))
                    return new List<CrdTreeNodeDto>();

                string codigo = IndiceCodigo(nodeKey);
                if (string.IsNullOrWhiteSpace(codigo))
                    return new List<CrdTreeNodeDto>();

                const string sqlGarantias = @"
                select T.garantia, T.descripcion
                from crd_catalogo_garantias C
                inner join crd_garantia_tipos T on C.garantia = T.garantia
                where C.codigo = @codigo";

                const string sqlDestinos = @"
                select cod_destino, descripcion
                from catalogo_destinos
                where cod_destino in (
                    select cod_destino
                    from CATALOGO_DESTINOSASG
                    where codigo = @codigo
                )";

                var garantias = conn.Query(sqlGarantias, new { codigo })
                    .Select(g => new
                    {
                        garantia = (g.garantia ?? "").ToString().Trim(),
                        descripcion = (g.descripcion ?? "").ToString().Trim()
                    })
                    .Where(x => x.garantia != "")
                    .ToList();

                var destinos = conn.Query(sqlDestinos, new { codigo })
                    .Select(d => new
                    {
                        cod_destino = (d.cod_destino ?? "").ToString().Trim(),
                        descripcion = (d.descripcion ?? "").ToString().Trim()
                    })
                    .Where(x => x.cod_destino != "")
                    .ToList();

                // VB6: por cada destino agrega el nodo D y dentro todas las garantias G
                var hijos = destinos.Select(dest =>
                {
                    var destinoKey = $"0x0{codigo}-{dest.cod_destino}D";

                    var hijosGarantias = garantias.Select(g => new CrdTreeNodeDto
                    {
                        key = $"{destinoKey}-{g.garantia}G",
                        label = g.descripcion,
                        leaf = true,
                        data = new { tipo = "G", codigo, cod_destino = dest.cod_destino, garantia = g.garantia }
                    }).ToList();

                    return new CrdTreeNodeDto
                    {
                        key = destinoKey,
                        label = $"{dest.cod_destino} - {dest.descripcion}",
                        leaf = hijosGarantias.Count == 0,
                        children = hijosGarantias,
                        data = new { tipo = "D", codigo, cod_destino = dest.cod_destino }
                    };
                }).ToList();

                return hijos;
            });
        }

        private static string IndiceCodigo(string nodeKey)
        {
            // VB6: Mid(xkey,4) y luego quitar último char (L/D/G)
            // "0x0ABC123L" -> "ABC123"
            if (string.IsNullOrWhiteSpace(nodeKey)) return "";
            if (!nodeKey.StartsWith("0x0") || nodeKey.Length < 5) return "";

            var tmp = nodeKey.Substring(3);               // quita "0x0"
            tmp = tmp.Substring(0, tmp.Length - 1);       // quita sufijo L/D/G
            return tmp;
        }


        /// <summary>
        /// Lista de pólizas para la asignación (equivalente a sbCargaLswAdicional VB6).
        /// Retorna todas las pólizas y marca asignado=true si existe vínculo en CRD_CATALOGO_POLIZAS_ASG.
        /// </summary>
        public ErrorDto<List<CrdCatalogoPolizasAsignacionDto>> Crd_CatalogoPolizas_Asignacion_Obtener(
            int CodEmpresa,
            string codigo,
            string cod_destino,
            string garantia)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                //elimino ultimo caracter del key para obtener el codigo (L/D/G)
                string codDestino = cod_destino.Substring(0, cod_destino.Length - 1);
                string codGarantia = garantia.Substring(0, garantia.Length - 1);

                const string query = @"
                        SELECT
                            R.cod_poliza,
                            R.descripcion,
                            R.tipo,
                            R.valor,
                            CASE WHEN A.codigo IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS asignado
                        FROM CRD_CATALOGO_POLIZAS R
                        LEFT JOIN CRD_CATALOGO_POLIZAS_ASG A
                            ON  R.cod_poliza  = A.cod_poliza
                            AND A.codigo      = @codigo
                            AND A.cod_destino = @cod_destino
                            AND A.garantia    = @garantia
                        ORDER BY asignado DESC, R.cod_poliza;";

                var response = conn.Query<CrdCatalogoPolizasAsignacionDto>(query, new
                {
                    codigo = (codigo ?? "").Trim(),
                    cod_destino = codDestino,
                    garantia = codGarantia
                }).ToList();

                return response;
            });
        }

        /// <summary>
        /// Asigna o desasigna una póliza a una combinación (codigo, destino, garantia).
        /// Equivalente a lsw_ItemCheck en VB6.
        /// </summary>
        public ErrorDto Crd_CatalogoPolizas_Asignacion_Actualizar(
            int CodEmpresa,
            string usuario,
            CrdCatalogoPolizasAsignacionUpdateDto datos)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                //elimino ultimo caracter del key para obtener el codigo (L/D/G)
                string codDestino = datos.cod_destino.Substring(0, datos.cod_destino.Length - 1);
                string codGarantia = datos.garantia.Substring(0, datos.garantia.Length - 1);

                if (datos.asignado)
                {
                    const string insertQuery = @"
                            IF NOT EXISTS (
                                SELECT 1
                                FROM CRD_CATALOGO_POLIZAS_ASG
                                WHERE cod_poliza = @cod_poliza
                                  AND codigo = @codigo
                                  AND cod_destino = @cod_destino
                                  AND garantia = @garantia
                            )
                            INSERT INTO CRD_CATALOGO_POLIZAS_ASG
                                (cod_poliza, codigo, cod_destino, garantia)
                            VALUES
                                (@cod_poliza, @codigo, @cod_destino, @garantia);";

                    connection.Execute(insertQuery, new
                    {
                        datos.cod_poliza,
                        datos.codigo,
                        cod_destino = codDestino,
                        garantia = codGarantia
                    });

                    return DbHelper.OkResponse("Asignación Guardada Correctamente");
                }
                else
                {
                    const string deleteQuery = @"
                            DELETE FROM CRD_CATALOGO_POLIZAS_ASG
                            WHERE cod_poliza = @cod_poliza
                              AND codigo = @codigo
                              AND cod_destino = @cod_destino
                              AND garantia = @garantia;";

                    connection.Execute(deleteQuery, new
                    {
                        datos.cod_poliza,
                        datos.codigo,
                        cod_destino = codDestino,
                        garantia = codGarantia
                    });

                    return DbHelper.OkResponse("Asignación Eliminada Correctamente");
                }

                
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error al actualizar asignación: " + ex.Message);
            }
        }


        #endregion


        #region Acreedores

        /// <summary>
        /// Lista de acreedores para el mantenimiento (VB6: tcMain_SelectedChanged Case 2 -> sbCargaGrid vGrid).
        /// </summary>
        public ErrorDto<List<CrdPolizasAcreedoresGridDto>> Crd_PolizasAcreedores_Grid_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        COD_ACREEDOR     AS cod_acreedor,
                        IDENTIFICACION   AS identificacion,
                        NOMBRE           AS nombre,
                        CXP_ENLACE       AS cxp_enlace,
                        ACTIVO           AS activo
                    FROM CRD_POLIZAS_ACREEDORES
                    ORDER BY COD_ACREEDOR";

                return conn.Query<CrdPolizasAcreedoresGridDto>(query).ToList();
            });
        }

        /// <summary>
        /// Método para eliminar un acreedor, con validación previa para evitar eliminar acreedores que estén asignados a pólizas activas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto Crd_PolizasAcreedores_Eliminar(
                int CodEmpresa,
                string usuario,
                string cod_acreedor)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                cod_acreedor = (cod_acreedor ?? string.Empty).Trim();

                const string validaSql = @"
                        SELECT COUNT(1)
                        FROM CRD_POLIZAS_ACREEDOR_ASG
                        WHERE cod_acreedor = @cod_acreedor;
                        ";

                var enUso = connection.ExecuteScalar<int>(validaSql, new { cod_acreedor });

                if (enUso > 0)
                {
                    response.Code = -1;
                    response.Description = $"No se puede eliminar: el acreedor está asignado a {enUso} póliza(s).";
                    return response;
                }

                const string deleteSql = @"
                        DELETE CRD_POLIZAS_ACREEDORES
                        WHERE COD_ACREEDOR = @cod_acreedor;
                        ";

                var rows = connection.Execute(deleteSql, new { cod_acreedor });

                if (rows > 0)
                {
                    Bitacora(CodEmpresa, usuario, $"Acreedor de Pólizas : {cod_acreedor}", "Elimina - WEB");
                }
                else
                {
                    response.Code = -1;
                    response.Description = "Error al eliminar acreedor.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al eliminar acreedor: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Guarda o actualiza un acreedor del catálogo (VB6: fxGuardar en vGrid).
        /// - Si existe COD_ACREEDOR: UPDATE
        /// - Si no existe: INSERT
        /// </summary>
        public ErrorDto Crd_PolizasAcreedores_Guardar(
            int CodEmpresa,
            string usuario,
            CrdPolizasAcreedoresGridSaveDto datos)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
          
            try
            {
                var cod = (datos?.cod_acreedor ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(cod))
                {
                    return DbHelper.ErrorResponse("El código del acreedor es requerido.");
                }

                var identificacion = (datos?.identificacion ?? string.Empty).Trim();
                var nombre = (datos?.nombre ?? string.Empty).Trim();
                var cxp = datos?.cxp_enlace;                 // puede ser null
                var activo = (datos?.activo ?? 1);           // default 1

                // Validaciones mínimas (ajusta a reglas reales)
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return DbHelper.ErrorResponse("La identificación es requerida.");
                }
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return DbHelper.ErrorResponse("El nombre es requerido.");
                }

                const string existsSql = @"
                        SELECT ISNULL(COUNT(1),0)
                        FROM CRD_POLIZAS_ACREEDORES
                        WHERE COD_ACREEDOR = @cod;
                        ";

                var existe = connection.ExecuteScalar<int>(existsSql, new { cod });

                var response = false;

                if (existe == 0)
                {
                    const string insertSql = @"
                            INSERT INTO CRD_POLIZAS_ACREEDORES
                                (COD_ACREEDOR, IDENTIFICACION, NOMBRE, CXP_ENLACE, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                            VALUES
                                (@cod, @identificacion, @nombre, @cxp_enlace, @activo, dbo.MyGetdate(), @usuario);
                            ";

                    var rows = connection.Execute(insertSql, new
                    {
                        cod,
                        identificacion,
                        nombre,
                        cxp_enlace = cxp,
                        activo,
                        usuario
                    });

                    response = rows > 0;
                }
                else
                {
                    const string updateSql = @"
                            UPDATE CRD_POLIZAS_ACREEDORES
                               SET IDENTIFICACION   = @identificacion,
                                   NOMBRE           = @nombre,
                                   CXP_ENLACE       = @cxp_enlace,
                                   ACTIVO           = @activo
                             WHERE COD_ACREEDOR     = @cod;
                            ";

                    var rows = connection.Execute(updateSql, new
                    {
                        cod,
                        identificacion,
                        nombre,
                        cxp_enlace = cxp,
                        activo
                    });

                    response = rows > 0;
                }

                if (!response)
                {
                    return DbHelper.ErrorResponse("No se pudo guardar el registro.");
                }
                Bitacora(CodEmpresa, usuario, $"Acreedor de Pólizas : {cod}", existe == 0 ? "Registra-Web" : "Modifica-Web");

                return DbHelper.OkResponse($"Acreedor {(existe == 0 ? "registrado" : "actualizado")} exitosamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al guardar acreedor: {ex.Message}");
            }
        }


        #endregion
    }
}
