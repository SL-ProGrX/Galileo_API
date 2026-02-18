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
