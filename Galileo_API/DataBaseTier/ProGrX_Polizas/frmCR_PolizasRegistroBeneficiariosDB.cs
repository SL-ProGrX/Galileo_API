using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using static Galileo_API.Models.ProGrX_Polizas.FrmCRPolizasRegistroBeneficiariosModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizasRegistroBeneficiariosDb
    {

        private readonly PortalDB _portalDb;


        public FrmCRPolizasRegistroBeneficiariosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de parentescos activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CRPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        RTRIM(cod_Parentesco) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM sys_Parentescos
                    WHERE activo = 1
                    ORDER BY Descripcion;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene el encabezado de la póliza seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <param name="NumPoliza"></param>
        /// <returns></returns>
        public ErrorDto<CRPolizasRegistroBeneficiariosEncabezadoResponse> CRPolizasRegistroBeneficiarios_Encabezado_Obtener(
            int codEmpresa,
            int IdSolicitud,
            int NumPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        Pol.Id_Solicitud AS IdSolicitud,
                        Pol.Num_Poliza AS NumPoliza,
                        ISNULL(Pol.Num_Contrato, '') AS NumContrato,
                        ISNULL(S.Cedula, '') AS CedulaDeudor,
                        ISNULL(S.Nombre, '') AS NombreDeudor,
                        ISNULL(R.Codigo, '') AS CodigoLinea,
                        ISNULL(C.Descripcion, '') AS LineaDescripcion,
                        ISNULL(Cat.descripcion, '') AS PolizaDescripcion,
                        ISNULL(CONVERT(VARCHAR(50), Pol.Id_Solicitud_Poliza), '') AS IdSolicitudPoliza
                    FROM CRD_OPERACION_POLIZAS Pol
                    INNER JOIN CRD_CATALOGO_POLIZAS Cat
                        ON Pol.cod_poliza = Cat.cod_poliza
                    INNER JOIN Reg_Creditos R
                        ON Pol.Id_Solicitud = R.id_Solicitud
                    INNER JOIN Catalogo C
                        ON R.codigo = C.codigo
                    INNER JOIN Socios S
                        ON R.cedula = S.cedula
                    WHERE Pol.id_Solicitud = @IdSolicitud
                      AND Pol.Num_Poliza = @NumPoliza;";

                return conn.QueryFirstOrDefault<CRPolizasRegistroBeneficiariosEncabezadoResponse>(query, new
                {
                    IdSolicitud,
                    NumPoliza
                }) ?? new CRPolizasRegistroBeneficiariosEncabezadoResponse();
            });
        }

        /// <summary>
        /// Obtiene el listado de beneficiarios de la póliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <param name="NumPoliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CRPolizasRegistroBeneficiariosListaItem>> CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
            int codEmpresa,
           int IdSolicitud,
            int NumPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        b.Num_Poliza AS NumPoliza,
                        RTRIM(ISNULL(b.Id_Beneficiario, '')) AS IdBeneficiario,
                        RTRIM(ISNULL(b.Nombre, '')) AS Nombre,
                        b.FechaNac AS FechaNacimiento,
                        RTRIM(ISNULL(p.Descripcion, '')) AS Parentesco,
                        ISNULL(b.Porcentaje, 0) AS Porcentaje
                    FROM CRD_OPERACION_POLIZAS_BENEFICIARIOS b
                    LEFT JOIN sys_Parentescos p
                        ON b.parentesco = p.cod_Parentesco
                    WHERE b.id_Solicitud = @IdSolicitud
                      AND b.num_Poliza = @NumPoliza
                    ORDER BY b.Id_Beneficiario;";

                return conn.Query<CRPolizasRegistroBeneficiariosListaItem>(query, new
                {
                    IdSolicitud,
                    NumPoliza
                }).ToList();
            });
        }

        /// <summary>
        ///  Obtiene el siguiente identificador sugerido para un nuevo beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <param name="NumPoliza"></param>
        /// <returns></returns>
        public ErrorDto<CRPolizasRegistroBeneficiariosNuevoResponse> CRPolizasRegistroBeneficiarios_Nuevo_Obtener(
            int codEmpresa,
            int IdSolicitud,
            int NumPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                     select isnull(count(*),0) + 1 as IdBeneficiarioSugerido , Getdate() as FechaServidor from CRD_OPERACION_POLIZAS_BENEFICIARIOS
                    WHERE Id_Solicitud = @IdSolicitud
                      AND Num_Poliza = @NumPoliza;";

                return conn.QueryFirstOrDefault<CRPolizasRegistroBeneficiariosNuevoResponse>(query, new
                {
                    IdSolicitud,
                    NumPoliza
                }) ?? new CRPolizasRegistroBeneficiariosNuevoResponse();
            });
        }


        private static void MapearNombreBeneficiario(CRPolizasRegistroBeneficiarios model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreCompleto))
            {
                return;
            }

            var partes = model.NombreCompleto
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            model.Apellido1 = partes.Length > 0 ? partes[0] : string.Empty;
            model.Apellido2 = partes.Length > 1 ? partes[1] : string.Empty;
            model.Nombre = partes.Length > 2 ? string.Join(" ", partes.Skip(2)) : string.Empty;
        }


        /// <summary>
        /// Obtiene el detalle de un beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <param name="NumPoliza"></param>
        /// <param name="IdBeneficiario"></param>
        /// <returns></returns>
        public ErrorDto<CRPolizasRegistroBeneficiarios> CRPolizasRegistroBeneficiarios_Detalle_Obtener(
            int codEmpresa,
            int IdSolicitud,
            int NumPoliza,
            string IdBeneficiario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        RTRIM(ISNULL(b.Id_Beneficiario, '')) AS IdBeneficiario,
                        RTRIM(ISNULL(b.Nombre, '')) AS NombreCompleto,
                        RTRIM(ISNULL(b.parentesco, '')) AS Parentesco,
                        RTRIM(ISNULL(p.Descripcion, '')) AS ParentescoDescripcion,
                        b.FechaNac AS FechaNacimiento,
                        ISNULL(b.Porcentaje, 0) AS Porcentaje,
                        ISNULL(b.Notas, '') AS Observacion,
                        ISNULL(b.Direccion, '') AS Direccion,
                        ISNULL(b.Apto_Postal, '') AS ApartadoPostal,
                        ISNULL(b.Email, '') AS Email,
                        ISNULL(b.Telefono1, '') AS Telefono1,
                        ISNULL(b.Telefono2, '') AS Telefono2
                    FROM CRD_OPERACION_POLIZAS_BENEFICIARIOS b
                    LEFT JOIN sys_Parentescos p
                        ON b.parentesco = p.cod_Parentesco
                    WHERE b.id_Solicitud = @IdSolicitud
                      AND b.num_Poliza = @NumPoliza
                      AND b.Id_Beneficiario = @IdBeneficiario;";

                var result = conn.QueryFirstOrDefault<CRPolizasRegistroBeneficiarios>(query, new
                {
                    IdSolicitud,
                    NumPoliza,
                    IdBeneficiario
                });

                if (result is null)
                {
                    return new CRPolizasRegistroBeneficiarios();
                }

                MapearNombreBeneficiario(result);

                return result;
            });
        }

        private static string ConstruirNombreCompleto(string apellido1, string apellido2, string nombre)
        {
            var partes = new[]
            {
                apellido1?.Trim().ToUpperInvariant(),
                apellido2?.Trim().ToUpperInvariant(),
                nombre?.Trim().ToUpperInvariant()
            }
            .Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(" ", partes);
        }

        private static string ValidarGuardar(
           IDbConnection connection,
           IDbTransaction transaction,
           CRPolizasRegistroBeneficiarios request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.IdBeneficiario))
            {
                errores.Add("La identificación del beneficiario es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(request.Parentesco))
            {
                errores.Add("Debe seleccionar un parentesco.");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                errores.Add("El nombre del beneficiario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Apellido1))
            {
                errores.Add("El primer apellido del beneficiario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Apellido2))
            {
                errores.Add("El segundo apellido del beneficiario es obligatorio.");
            }

            if (request.Porcentaje <= 0)
            {
                errores.Add("El porcentaje debe ser mayor a 0.");
            }

            if (request.Porcentaje > 100)
            {
                errores.Add("El porcentaje no puede ser mayor a 100.");
            }

            if (!request.EsEdicion)
            {
                const string existeQuery = @"
                    SELECT ISNULL(COUNT(*), 0)
                    FROM CRD_OPERACION_POLIZAS_BENEFICIARIOS
                    WHERE id_Solicitud = @IdSolicitud
                      AND Num_Poliza = @NumPoliza
                      AND Id_Beneficiario = @IdBeneficiario;";

                var existe = connection.ExecuteScalar<int>(existeQuery, new
                {
                    request.IdSolicitud,
                    request.NumPoliza,
                    request.IdBeneficiario
                }, transaction);

                if (existe > 0)
                {
                    errores.Add("Ya existe un beneficiario registrado con la misma cédula.");
                }
            }

            const string porcentajeQuery = @"
                SELECT ISNULL(SUM(Porcentaje), 0)
                FROM CRD_OPERACION_POLIZAS_BENEFICIARIOS
                WHERE id_Solicitud = @IdSolicitud
                  AND Num_Poliza = @NumPoliza
                  AND Id_Beneficiario <> @IdBeneficiarioExcluir;";

            var porcentajeActual = connection.ExecuteScalar<decimal>(porcentajeQuery, new
            {
                request.IdSolicitud,
                request.NumPoliza,
                IdBeneficiarioExcluir = request.EsEdicion
                    ? request.IdBeneficiarioOriginal
                    : string.Empty
            }, transaction);

            if ((porcentajeActual + request.Porcentaje) > 100)
            {
                errores.Add("El porcentaje sobrepasa el total permitido del 100%.");
            }

            return string.Join(" ", errores);
        }

        /// <summary>
        /// Inserta o actualiza un beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CRPolizasRegistroBeneficiariosGuardarResponse> CRPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CRPolizasRegistroBeneficiarios request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                var validacion = ValidarGuardar(connection, transaction, request);
                if (!string.IsNullOrWhiteSpace(validacion))
                {
                    transaction.Rollback();


                    return DbHelper.CreateErrorResponse<CRPolizasRegistroBeneficiariosGuardarResponse>(
                        validacion,
                        -2,
                        new CRPolizasRegistroBeneficiariosGuardarResponse
                        {
                            Guardado = false,
                            IdBeneficiario = request.IdBeneficiario
                        });
                }

                var nombreCompleto = ConstruirNombreCompleto(
                    request.Apellido1,
                    request.Apellido2,
                    request.Nombre);

                if (request.EsEdicion)
                {
                    const string updateQuery = @"
                        UPDATE CRD_OPERACION_POLIZAS_BENEFICIARIOS
                        SET Nombre = @Nombre,
                            Id_Beneficiario = @IdBeneficiario,
                            parentesco = @Parentesco,
                            notas = @Observacion,
                            direccion = @Direccion,
                            apto_postal = @ApartadoPostal,
                            email = @Email,
                            telefono1 = @Telefono1,
                            telefono2 = @Telefono2,
                            fechaNac = @FechaNacimiento,
                            porcentaje = @Porcentaje
                        WHERE id_Solicitud = @IdSolicitud
                          AND Num_Poliza = @NumPoliza
                          AND id_Beneficiario = @IdBeneficiarioOriginal;";

                    connection.Execute(updateQuery, new
                    {
                        request.IdSolicitud,
                        request.NumPoliza,
                        request.IdBeneficiarioOriginal,
                        request.IdBeneficiario,
                        Nombre = nombreCompleto,
                        request.Parentesco,
                        request.Observacion,
                        request.Direccion,
                        request.ApartadoPostal,
                        request.Email,
                        request.Telefono1,
                        request.Telefono2,
                        FechaNacimiento = request.FechaNacimiento.Date,
                        request.Porcentaje
                    }, transaction);
                }
                else
                {
                    const string insertQuery = @"
                        INSERT INTO CRD_OPERACION_POLIZAS_BENEFICIARIOS
                        (
                            id_solicitud,
                            codigo,
                            num_Poliza,
                            Id_Beneficiario,
                            Nombre,
                            parentesco,
                            fechaNac,
                            porcentaje,
                            direccion,
                            notas,
                            telefono1,
                            telefono2,
                            email,
                            apto_postal,
                            registro_usuario,
                            registro_fecha
                        )
                        VALUES
                        (
                            @IdSolicitud,
                            @CodigoLinea,
                            @NumPoliza,
                            @IdBeneficiario,
                            @Nombre,
                            @Parentesco,
                            @FechaNacimiento,
                            @Porcentaje,
                            @Direccion,
                            @Observacion,
                            @Telefono1,
                            @Telefono2,
                            @Email,
                            @ApartadoPostal,
                            @Usuario,
                            dbo.MyGetDate()
                        );";

                    connection.Execute(insertQuery, new
                    {
                        request.IdSolicitud,
                        request.CodigoLinea,
                        request.NumPoliza,
                        request.IdBeneficiario,
                        Nombre = nombreCompleto,
                        request.Parentesco,
                        FechaNacimiento = request.FechaNacimiento.Date,
                        request.Porcentaje,
                        request.Direccion,
                        request.Observacion,
                        request.Telefono1,
                        request.Telefono2,
                        request.Email,
                        request.ApartadoPostal,
                        Usuario = usuario
                    }, transaction);
                }

                transaction.Commit();

                return DbHelper.CreateOkResponse(new CRPolizasRegistroBeneficiariosGuardarResponse
                {
                    Guardado = true,
                    IdBeneficiario = request.IdBeneficiario,
                    Mensaje = "Información guardada satisfactoriamente."
                });
            }
            catch (Exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    "Error al guardar la información del beneficiario.",
                    -1,
                    new CRPolizasRegistroBeneficiariosGuardarResponse
                    {
                        Guardado = false,
                        IdBeneficiario = request.IdBeneficiario
                    });
            }
        }


        /// <summary>
        /// Elimina un beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <param name="NumPoliza"></param>
        /// <param name="IdBeneficiario"></param>
        /// <returns></returns>
        public ErrorDto CRPolizasRegistroBeneficiarios_Eliminar(
            int codEmpresa,
            int IdSolicitud,
            int NumPoliza,
            string IdBeneficiario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                    DELETE FROM CRD_OPERACION_POLIZAS_BENEFICIARIOS
                    WHERE id_Solicitud = @IdSolicitud
                      AND Num_Poliza = @NumPoliza
                      AND id_Beneficiario = @IdBeneficiario;";

                connection.Execute(query, new
                {
                    IdSolicitud,
                    NumPoliza,
                    IdBeneficiario
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse(
                    "Error al eliminar el beneficiario.",
                    -1);
            }
        }

    }
}
