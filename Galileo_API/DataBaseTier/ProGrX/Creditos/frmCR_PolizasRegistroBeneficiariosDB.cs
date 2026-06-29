using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRPolizasRegistroBeneficiariosDB
    {
        private readonly PortalDB _portalDb;

        private const string MensajeOperacionPolizaRequerida = "Debe indicar la operaci&oacute;n y el n&uacute;mero de p&oacute;liza.";
        private const string MensajeBeneficiarioRequerido = "Debe indicar el beneficiario.";
        private const string MensajeGuardarOk = "Informaci&oacute;n guardada satisfactoriamente.";
        private const string MensajeEliminarOk = "Beneficiario eliminado satisfactoriamente.";

        public FrmCRPolizasRegistroBeneficiariosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de parentescos activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    rtrim(cod_parentesco) as item,
                    rtrim(descripcion) as descripcion
                from sys_parentescos
                where activo = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene el encabezado principal de la poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroBeneficiariosEncabezadoData?> CrPolizasRegistroBeneficiarios_Encabezado_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
        {
            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    (CrPolizasRegistroBeneficiariosEncabezadoData?)null);
            }

            const string query = @"
                select top 1
                    pol.id_solicitud as operacion,
                    pol.num_poliza as num_poliza,
                    rtrim(isnull(pol.num_contrato, '')) as num_contrato,
                    rtrim(isnull(s.cedula, '')) as cedula_deudor,
                    rtrim(isnull(s.nombre, '')) as nombre_deudor,
                    rtrim(isnull(r.codigo, '')) as codigo_linea,
                    rtrim(isnull(c.descripcion, '')) as linea_descripcion,
                    rtrim(isnull(cat.descripcion, '')) as poliza_descripcion,
                    isnull(pol.id_solicitud_poliza, 0) as id_solicitud_poliza
                from crd_operacion_polizas pol
                inner join crd_catalogo_polizas cat
                    on pol.cod_poliza = cat.cod_poliza
                inner join reg_creditos r
                    on pol.id_solicitud = r.id_solicitud
                inner join catalogo c
                    on r.codigo = c.codigo
                inner join socios s
                    on r.cedula = s.cedula
                where pol.id_solicitud = @Operacion
                  and pol.num_poliza = @NumPoliza;";

            var response = DbHelper.ExecuteSingleQuery<CrPolizasRegistroBeneficiariosEncabezadoData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza
                });

            if (response.Code == 0 && response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontr&oacute; la p&oacute;liza seleccionada.",
                    -2,
                    (CrPolizasRegistroBeneficiariosEncabezadoData?)null);
            }

            return response;
        }

        /// <summary>
        /// Obtiene el listado de beneficiarios de la poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroBeneficiariosListaData>> CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
        {
            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    new List<CrPolizasRegistroBeneficiariosListaData>());
            }

            const string query = @"
                select
                    isnull(b.num_poliza, 0) as num_poliza,
                    rtrim(isnull(b.id_beneficiario, '')) as id_beneficiario,
                    rtrim(isnull(b.nombre, '')) as nombre,
                    b.fechanac as fecha_nac,
                    rtrim(isnull(b.parentesco, '')) as parentesco,
                    rtrim(isnull(p.descripcion, '')) as parentesco_descripcion,
                    isnull(b.porcentaje, 0) as porcentaje,
                    rtrim(isnull(b.direccion, '')) as direccion,
                    rtrim(isnull(b.notas, '')) as notas,
                    rtrim(isnull(b.telefono1, '')) as telefono1,
                    rtrim(isnull(b.telefono2, '')) as telefono2,
                    rtrim(isnull(b.email, '')) as email,
                    rtrim(isnull(b.apto_postal, '')) as apto_postal
                from crd_operacion_polizas_beneficiarios b
                left join sys_parentescos p
                    on b.parentesco = p.cod_parentesco
                where b.id_solicitud = @Operacion
                  and b.num_poliza = @NumPoliza
                order by b.id_beneficiario;";

            return DbHelper.ExecuteListQuery<CrPolizasRegistroBeneficiariosListaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza
                });
        }

        /// <summary>
        /// Obtiene la identificación sugerida para un nuevo beneficiario y la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroBeneficiariosNuevoData?> CrPolizasRegistroBeneficiarios_Nuevo_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
        {
            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    (CrPolizasRegistroBeneficiariosNuevoData?)null);
            }

            const string query = @"
                select top 1
                    rtrim(isnull(s.cedula, '')) + '-' +
                    right('00' + convert(varchar(10), isnull(x.consec, 1)), 2) as id_beneficiario_sugerido,
                    getdate() as fecha_servidor
                from crd_operacion_polizas pol
                inner join reg_creditos r
                    on pol.id_solicitud = r.id_solicitud
                inner join socios s
                    on r.cedula = s.cedula
                outer apply
                (
                    select isnull(count(*), 0) + 1 as consec
                    from crd_operacion_polizas_beneficiarios
                    where id_solicitud = pol.id_solicitud
                      and num_poliza = pol.num_poliza
                ) x
                where pol.id_solicitud = @Operacion
                  and pol.num_poliza = @NumPoliza;";

            var response = DbHelper.ExecuteSingleQuery<CrPolizasRegistroBeneficiariosNuevoData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza
                });

            if (response.Code == 0 && response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible generar la identificaci&oacute;n sugerida.",
                    -2,
                    (CrPolizasRegistroBeneficiariosNuevoData?)null);
            }

            return response;
        }

        /// <summary>
        /// Obtiene el detalle de un beneficiario especifico.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <param name="idBeneficiario"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroBeneficiariosDetalleData?> CrPolizasRegistroBeneficiarios_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
        {
            idBeneficiario = NormalizarTexto(idBeneficiario);

            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    (CrPolizasRegistroBeneficiariosDetalleData?)null);
            }

            if (string.IsNullOrWhiteSpace(idBeneficiario))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeBeneficiarioRequerido,
                    -2,
                    (CrPolizasRegistroBeneficiariosDetalleData?)null);
            }

            const string query = @"
                select top 1
                    isnull(b.id_solicitud, 0) as operacion,
                    isnull(b.num_poliza, 0) as num_poliza,
                    rtrim(isnull(b.codigo, '')) as codigo_linea,
                    rtrim(isnull(b.id_beneficiario, '')) as id_beneficiario_original,
                    rtrim(isnull(b.id_beneficiario, '')) as id_beneficiario,
                    rtrim(isnull(b.nombre, '')) as nombre_completo,
                    rtrim(isnull(b.parentesco, '')) as parentesco,
                    rtrim(isnull(p.descripcion, '')) as parentesco_descripcion,
                    b.fechanac as fecha_nacimiento,
                    isnull(b.porcentaje, 0) as porcentaje,
                    rtrim(isnull(b.notas, '')) as observacion,
                    rtrim(isnull(b.direccion, '')) as direccion,
                    rtrim(isnull(b.apto_postal, '')) as apartado_postal,
                    rtrim(isnull(b.email, '')) as email,
                    rtrim(isnull(b.telefono1, '')) as telefono1,
                    rtrim(isnull(b.telefono2, '')) as telefono2
                from crd_operacion_polizas_beneficiarios b
                left join sys_parentescos p
                    on b.parentesco = p.cod_parentesco
                where b.id_solicitud = @Operacion
                  and b.num_poliza = @NumPoliza
                  and b.id_beneficiario = @IdBeneficiario;";

            var response = DbHelper.ExecuteSingleQuery<CrPolizasRegistroBeneficiariosDetalleData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza,
                    IdBeneficiario = idBeneficiario
                });

            if (response.Code != 0 || response.Result is null)
            {
                return response.Code == 0
                    ? DbHelper.CreateErrorResponse(
                        "No se encontr&oacute; el beneficiario seleccionado.",
                        -2,
                        (CrPolizasRegistroBeneficiariosDetalleData?)null)
                    : response;
            }

            var nombres = SepararNombreBeneficiario(response.Result.nombre_completo);
            response.Result.apellido1 = nombres.apellido1;
            response.Result.apellido2 = nombres.apellido2;
            response.Result.nombre = nombres.nombre;

            return response;
        }

        /// <summary>
        /// Obtiene la lista para busqueda por nombre.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Busqueda_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
        {
            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string query = @"
                select
                    rtrim(isnull(id_beneficiario, '')) as item,
                    rtrim(isnull(nombre, '')) as descripcion
                from crd_operacion_polizas_beneficiarios
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza
                order by nombre;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza
                });
        }

        /// <summary>
        /// Inserta o actualiza un beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroBeneficiariosGuardarData> CrPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CrPolizasRegistroBeneficiariosGuardarRequest request)
        {
            usuario = NormalizarTexto(usuario);
            NormalizarGuardarRequest(request);

            var validacionBasica = ValidarGuardarBasico(usuario, request);
            if (validacionBasica is not null)
            {
                return validacionBasica;
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                var validacionNegocio = ValidarGuardarNegocio(connection, transaction, request);
                if (!string.IsNullOrWhiteSpace(validacionNegocio))
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse(
                        validacionNegocio,
                        -2,
                        new CrPolizasRegistroBeneficiariosGuardarData
                        {
                            guardado = false,
                            id_beneficiario = request.id_beneficiario
                        });
                }

                string nombreCompleto = ConstruirNombreCompleto(
                    request.apellido1,
                    request.apellido2,
                    request.nombre);

                if (request.es_edicion)
                {
                    const string updateQuery = @"
                        update crd_operacion_polizas_beneficiarios
                           set nombre = @Nombre,
                               id_beneficiario = @IdBeneficiario,
                               parentesco = @Parentesco,
                               notas = @Observacion,
                               direccion = @Direccion,
                               apto_postal = @ApartadoPostal,
                               email = @Email,
                               telefono1 = @Telefono1,
                               telefono2 = @Telefono2,
                               fechanac = @FechaNacimiento,
                               porcentaje = @Porcentaje
                         where id_solicitud = @Operacion
                           and num_poliza = @NumPoliza
                           and id_beneficiario = @IdBeneficiarioOriginal;";

                    connection.Execute(updateQuery, new
                    {
                        Operacion = request.operacion,
                        NumPoliza = request.num_poliza,
                        IdBeneficiarioOriginal = request.id_beneficiario_original,
                        IdBeneficiario = request.id_beneficiario,
                        Nombre = nombreCompleto,
                        Parentesco = request.parentesco,
                        Observacion = request.observacion,
                        Direccion = request.direccion,
                        ApartadoPostal = request.apartado_postal,
                        Email = request.email,
                        Telefono1 = request.telefono1,
                        Telefono2 = request.telefono2,
                        FechaNacimiento = request.fecha_nacimiento?.Date,
                        Porcentaje = request.porcentaje ?? 0m
                    }, transaction);
                }
                else
                {
                    const string insertQuery = @"
                        insert into crd_operacion_polizas_beneficiarios
                        (
                            id_solicitud,
                            codigo,
                            num_poliza,
                            id_beneficiario,
                            nombre,
                            parentesco,
                            fechanac,
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
                        values
                        (
                            @Operacion,
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
                            Getdate()
                        );";

                    connection.Execute(insertQuery, new
                    {
                        Operacion = request.operacion,
                        CodigoLinea = request.codigo_linea,
                        NumPoliza = request.num_poliza,
                        IdBeneficiario = request.id_beneficiario,
                        Nombre = nombreCompleto,
                        Parentesco = request.parentesco,
                        FechaNacimiento = request.fecha_nacimiento?.Date,
                        Porcentaje = request.porcentaje ?? 0m,
                        Direccion = request.direccion,
                        Observacion = request.observacion,
                        Telefono1 = request.telefono1,
                        Telefono2 = request.telefono2,
                        Email = request.email,
                        ApartadoPostal = request.apartado_postal,
                        Usuario = usuario
                    }, transaction);
                }

                transaction.Commit();

                return DbHelper.CreateOkResponse(new CrPolizasRegistroBeneficiariosGuardarData
                {
                    guardado = true,
                    id_beneficiario = request.id_beneficiario,
                    mensaje = MensajeGuardarOk
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrPolizasRegistroBeneficiariosGuardarData
                    {
                        guardado = false,
                        id_beneficiario = request.id_beneficiario
                    });
            }
        }

        /// <summary>
        /// Elimina un beneficiario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <param name="idBeneficiario"></param>
        /// <returns></returns>
        public ErrorDto CrPolizasRegistroBeneficiarios_Eliminar(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
        {
            idBeneficiario = NormalizarTexto(idBeneficiario);

            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.ErrorResponse(MensajeOperacionPolizaRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(idBeneficiario))
            {
                return DbHelper.ErrorResponse(MensajeBeneficiarioRequerido, -2);
            }

            const string query = @"
                delete from crd_operacion_polizas_beneficiarios
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza
                  and id_beneficiario = @IdBeneficiario;";

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza,
                    IdBeneficiario = idBeneficiario
                });

            if (response.Code != 0)
            {
                return response;
            }

            return new ErrorDto
            {
                Code = 0,
                Description = MensajeEliminarOk
            };
        }

        private static void NormalizarGuardarRequest(CrPolizasRegistroBeneficiariosGuardarRequest request)
        {
            request.codigo_linea = NormalizarTexto(request.codigo_linea);
            request.id_beneficiario_original = NormalizarTexto(request.id_beneficiario_original);
            request.id_beneficiario = NormalizarTexto(request.id_beneficiario);
            request.apellido1 = NormalizarTexto(request.apellido1);
            request.apellido2 = NormalizarTexto(request.apellido2);
            request.nombre = NormalizarTexto(request.nombre);
            request.parentesco = NormalizarTexto(request.parentesco);
            request.observacion = NormalizarTexto(request.observacion);
            request.direccion = NormalizarTexto(request.direccion);
            request.apartado_postal = NormalizarTexto(request.apartado_postal);
            request.email = NormalizarTexto(request.email);
            request.telefono1 = NormalizarTexto(request.telefono1);
            request.telefono2 = NormalizarTexto(request.telefono2);
        }

        private static ErrorDto<CrPolizasRegistroBeneficiariosGuardarData>? ValidarGuardarBasico(
            string usuario,
            CrPolizasRegistroBeneficiariosGuardarRequest request)
        {
            if (request.operacion <= 0 || request.num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionPolizaRequerida,
                    -2,
                    new CrPolizasRegistroBeneficiariosGuardarData
                    {
                        guardado = false,
                        id_beneficiario = request.id_beneficiario
                    });
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new CrPolizasRegistroBeneficiariosGuardarData
                    {
                        guardado = false,
                        id_beneficiario = request.id_beneficiario
                    });
            }

            return null;
        }

        private static string ValidarGuardarNegocio(
            IDbConnection connection,
            IDbTransaction transaction,
            CrPolizasRegistroBeneficiariosGuardarRequest request)
        {
            List<string> errores = new();

            if (string.IsNullOrWhiteSpace(request.id_beneficiario))
            {
                errores.Add("- La identificaci&oacute;n del beneficiario es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(request.parentesco))
            {
                errores.Add("- No se ha seleccionado ning&uacute;n parentesco...");
            }

            if (string.IsNullOrWhiteSpace(request.nombre))
            {
                errores.Add("- Nombre del beneficiario no es v&aacute;lido ...");
            }

            if (string.IsNullOrWhiteSpace(request.apellido1))
            {
                errores.Add("- Apellido 1 del beneficiario no es v&aacute;lido ...");
            }

            if (string.IsNullOrWhiteSpace(request.apellido2))
            {
                errores.Add("- Apellido 2 del beneficiario no es v&aacute;lido ...");
            }

            if (!request.porcentaje.HasValue || request.porcentaje <= 0)
            {
                errores.Add("- El porcentaje no es v&aacute;lido ...");
            }

            if (!request.es_edicion)
            {
                const string existeQuery = @"
                    select isnull(count(*), 0)
                    from crd_operacion_polizas_beneficiarios
                    where id_solicitud = @Operacion
                      and num_poliza = @NumPoliza
                      and id_beneficiario = @IdBeneficiario;";

                int existe = connection.ExecuteScalar<int>(existeQuery, new
                {
                    Operacion = request.operacion,
                    NumPoliza = request.num_poliza,
                    IdBeneficiario = request.id_beneficiario
                }, transaction);

                if (existe > 0)
                {
                    errores.Add("- Ya existe un beneficiario registrado con la misma c&eacute;dula ...");
                }
            }

            if (request.porcentaje.HasValue)
            {
                const string porcentajeQuery = @"
                    select isnull(sum(porcentaje), 0)
                    from crd_operacion_polizas_beneficiarios
                    where id_solicitud = @Operacion
                      and num_poliza = @NumPoliza
                      and id_beneficiario <> @IdBeneficiarioExcluir;";

                decimal porcentajeActual = connection.ExecuteScalar<decimal>(porcentajeQuery, new
                {
                    Operacion = request.operacion,
                    NumPoliza = request.num_poliza,
                    IdBeneficiarioExcluir = request.es_edicion
                        ? request.id_beneficiario_original
                        : string.Empty
                }, transaction);

                if ((request.porcentaje.Value + porcentajeActual) > 100m)
                {
                    errores.Add("- El porcentaje sobre pasa el total del 100% de los beneficiarios ...");
                }
            }

            return string.Join("\n", errores);
        }

        private static string ConstruirNombreCompleto(string apellido1, string apellido2, string nombre)
        {
            IEnumerable<string> partes = new[]
            {
                NormalizarTexto(apellido1).ToUpperInvariant(),
                NormalizarTexto(apellido2).ToUpperInvariant(),
                NormalizarTexto(nombre).ToUpperInvariant()
            }.Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(" ", partes);
        }

        private static CrPolizasRegistroBeneficiariosNombreData SepararNombreBeneficiario(string nombreCompleto)
        {
            string nombreNormalizado = NormalizarTexto(nombreCompleto);
            string[] partes = nombreNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return new CrPolizasRegistroBeneficiariosNombreData
            {
                apellido1 = partes.Length > 0 ? partes[0] : string.Empty,
                apellido2 = partes.Length > 1 ? partes[1] : string.Empty,
                nombre = partes.Length > 2 ? string.Join(" ", partes.Skip(2)) : string.Empty
            };
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }
    }
}