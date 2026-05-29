using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasTiposDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string EliminadoExitoso = "Informacion eliminada satisfactoriamente..."; 

        public FrmCrPrendasTiposDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrPrendasTiposDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene el catalogo de tipos de prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasTipoData>> CrPrendasTipos_Obtener(int codEmpresa)
        {
            const string SqlTiposObtener = @"
            select
                rtrim(TIPO_PRENDA) as tipo_prenda,
                rtrim(isnull(DESCRIPCION, '')) as descripcion,
                rtrim(isnull(FORMULARIO, '')) as formulario,
                cast(isnull(PORC_COBERTURA, 0) as decimal(18, 2)) as porc_cobertura,
                cast(isnull(ACTIVA, 0) as bit) as activa
            from CRD_PRENDAS_TIPOS
            order by TIPO_PRENDA;";

            return DbHelper.ExecuteListQuery<CrPrendasTipoData>(
                _portalDb,
                codEmpresa,
                SqlTiposObtener
            );
        }

        /// <summary>
        /// Guarda un tipo de prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPrendasTipos_Guardar(int codEmpresa, CrPrendasTipoGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.tipo.tipo_prenda = Limpiar(request.tipo.tipo_prenda);
            request.tipo.descripcion = (request.tipo.descripcion ?? string.Empty).Trim();
            request.tipo.formulario = (request.tipo.formulario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.tipo.tipo_prenda))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el tipo de prenda."
                };
            }

            if (ExisteTipo(codEmpresa, request.tipo.tipo_prenda))
            {
                return ActualizarTipo(codEmpresa, request);
            }

            return InsertarTipo(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un tipo de prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPrendasTipos_Eliminar(int codEmpresa, CrPrendasTipoEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.tipo_prenda = Limpiar(request.tipo_prenda);

            if (string.IsNullOrWhiteSpace(request.tipo_prenda))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el tipo de prenda."
                };
            }

            const string SqlTipoDelete = @"
            delete from CRD_PRENDAS_TIPOS
            where TIPO_PRENDA = @TipoPrenda;";
            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlTipoDelete,
                new
                {
                    TipoPrenda = request.tipo_prenda
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina",
                $"Tipo de Prenda: {request.tipo_prenda}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = EliminadoExitoso
            };
        }

        /// <summary>
        /// Obtiene las asignaciones para un tipo de prenda y categoria.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasTipoAsignacionData>> CrPrendasTipos_Asignacion_Obtener(
            int codEmpresa,
            CrPrendasTipoAsignacionObtenerRequest request)
        {
            request.tipo_prenda = Limpiar(request.tipo_prenda);
            request.categoria = Limpiar(request.categoria);
            request.filtro = (request.filtro ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.tipo_prenda))
            {
                return new ErrorDto<List<CrPrendasTipoAsignacionData>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new List<CrPrendasTipoAsignacionData>()
                };
            }

            const string SqlAsignacionObtener = @"
            exec spCrd_Prendas_Cat_List_Asignacion
                @TipoPrenda,
                @Categoria,
                @Filtro;";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<CrPrendasTipoAsignacionData>(
                    SqlAsignacionObtener,
                    new
                    {
                        TipoPrenda = request.tipo_prenda,
                        Categoria = request.categoria,
                        Filtro = request.filtro
                    }).ToList());

            return new ErrorDto<List<CrPrendasTipoAsignacionData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<CrPrendasTipoAsignacionData>()
            };
        }

        /// <summary>
        /// Guarda una asignacion para el tipo de prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPrendasTipos_Asignacion_Guardar(
            int codEmpresa,
            CrPrendasTipoAsignacionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.tipo_prenda = Limpiar(request.tipo_prenda);
            request.categoria = Limpiar(request.categoria);
            request.idx = (request.idx ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.tipo_prenda))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el tipo de prenda."
                };
            }

            if (string.IsNullOrWhiteSpace(request.categoria))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la categoria."
                };
            }

            if (string.IsNullOrWhiteSpace(request.idx))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el identificador de la asignacion."
                };
            }
            const string SqlAsignacionGuardar = @"
            exec spCrd_Prendas_Cat_List_Asignacion_Add
                @TipoPrenda,
                @Categoria,
                @Idx,
                @Usuario,
                @Accion;";
            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlAsignacionGuardar,
                new
                {
                    TipoPrenda = request.tipo_prenda,
                    Categoria = request.categoria,
                    Idx = request.idx,
                    Usuario = request.usuario,
                    Accion = request.asignado ? "A" : "E"
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto InsertarTipo(int codEmpresa, CrPrendasTipoGuardarRequest request)
        {
            const string SqlTipoInsert = @"
            insert into CRD_PRENDAS_TIPOS
            (
                TIPO_PRENDA,
                DESCRIPCION,
                FORMULARIO,
                PORC_COBERTURA,
                ACTIVA,
                REGISTRO_USUARIO,
                REGISTRO_FECHA
            )
            values
            (
                @TipoPrenda,
                @Descripcion,
                @Formulario,
                @PorcCobertura,
                @Activa,
                @Usuario,
               Getdate()
            );";
            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlTipoInsert,
                CrearParametrosTipo(request)
            );

            return FinalizarGuardado(
                codEmpresa,
                request.usuario,
                request.tipo.tipo_prenda,
                "Registra",
                resp
            );
        }

        private ErrorDto ActualizarTipo(int codEmpresa, CrPrendasTipoGuardarRequest request)
        {
            const string SqlTipoUpdate = @"
            update CRD_PRENDAS_TIPOS
            set DESCRIPCION = @Descripcion,
                FORMULARIO = @Formulario,
                PORC_COBERTURA = @PorcCobertura,
                ACTIVA = @Activa
            where TIPO_PRENDA = @TipoPrenda;";
            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlTipoUpdate,
                CrearParametrosTipo(request)
            );

            return FinalizarGuardado(
                codEmpresa,
                request.usuario,
                request.tipo.tipo_prenda,
                "Modifica",
                resp
            );
        }

        private static object CrearParametrosTipo(CrPrendasTipoGuardarRequest request)
        {
            return new
            {
                TipoPrenda = request.tipo.tipo_prenda,
                Descripcion = request.tipo.descripcion,
                Formulario = request.tipo.formulario,
                PorcCobertura = request.tipo.porc_cobertura,
                Activa = request.tipo.activa ? 1 : 0,
                Usuario = request.usuario
            };
        }

        private ErrorDto FinalizarGuardado(
            int codEmpresa,
            string usuario,
            string tipoPrenda,
            string movimiento,
            ErrorDto resp)
        {
            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento,
                $"Tipo de Prenda: {tipoPrenda}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private bool ExisteTipo(int codEmpresa, string tipoPrenda)
        {
            const string SqlTipoExiste = @"
                select coalesce(count(*), 0)
                from CRD_PRENDAS_TIPOS
                where TIPO_PRENDA = @TipoPrenda;";
            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                SqlTipoExiste,
                0,
                new
                {
                    TipoPrenda = tipoPrenda
                }
            );

            return resp.Result > 0;
        }

        private static string Limpiar(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}