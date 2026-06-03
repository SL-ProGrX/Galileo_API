using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPlazosBonificacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string EliminadoExitoso = "Informacion eliminada satisfactoriamente...";

        public FrmCrPlazosBonificacionDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrPlazosBonificacionDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene los planes registrados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPlazosBonificacionPlanData>> CrPlazosBonificacion_Planes_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(isnull(cod_Plazo_Bono, '')) as cod_plazo_bono,
                    rtrim(isnull(Descripcion, '')) as descripcion,
                    rtrim(isnull(Notas, '')) as notas,
                    cast(isnull(Activo, 0) as bit) as activo,
                    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
                    Registro_Fecha as registro_fecha
                from CRD_PLAZO_BONO
                order by cod_Plazo_Bono;";

            return DbHelper.ExecuteListQuery<CrPlazosBonificacionPlanData>(
                _portalDb,
                codEmpresa,
                sql
            );
        }

        /// <summary>
        /// Obtiene el siguiente o anterior plan registrado, dependiendo del valor de scroll 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="codPlazoBono"></param>
        /// <returns></returns>
        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Scroll_Obtener(
            int codEmpresa,
            int scroll,
            string codPlazoBono)
        {
            codPlazoBono = Limpiar(codPlazoBono);

            string sqlNext = scroll == 1
                ? @"select top 1 cod_Plazo_Bono
            from CRD_PLAZO_BONO
            where cod_Plazo_Bono > @CodPlazoBono
            order by cod_Plazo_Bono asc;"
                : @"select top 1 cod_Plazo_Bono
            from CRD_PLAZO_BONO
            where cod_Plazo_Bono < @CodPlazoBono
            order by cod_Plazo_Bono desc;";

            var nextId = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sqlNext,
                null,
                new
                {
                    CodPlazoBono = codPlazoBono
                }
            );

            if (!string.IsNullOrWhiteSpace(nextId.Result))
            {
                return CrPlazosBonificacion_Definicion_Obtener(codEmpresa, nextId.Result);
            }

            return new ErrorDto<CrPlazosBonificacionDefinicionData?>
            {
                Code = -2,
                Description = "No se encontraron mas resultados.",
                Result = null
            };
        }

        /// <summary>
        /// Obtiene la definición de un plan.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlazoBono"></param>
        /// <returns></returns>
        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Definicion_Obtener(
            int codEmpresa,
            string codPlazoBono)
        {
            codPlazoBono = Limpiar(codPlazoBono);

            const string sql = @"
                select
                    rtrim(isnull(cod_Plazo_Bono, '')) as cod_plazo_bono,
                    rtrim(isnull(Descripcion, '')) as descripcion,
                    rtrim(isnull(Notas, '')) as notas,
                    cast(isnull(Activo, 0) as bit) as activo
                from CRD_PLAZO_BONO
                where cod_Plazo_Bono = @CodPlazoBono;";

            return DbHelper.ExecuteSingleQuery<CrPlazosBonificacionDefinicionData>(
                _portalDb,
                codEmpresa,
                sql,
                new CrPlazosBonificacionDefinicionData(),
                new
                {
                    CodPlazoBono = codPlazoBono
                }
            );
        }

        /// <summary>
        /// Guarda la definición de un plan.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPlazosBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.codigo_original = Limpiar(request.codigo_original);
            request.definicion.cod_plazo_bono = Limpiar(request.definicion.cod_plazo_bono);
            request.definicion.descripcion = (request.definicion.descripcion ?? string.Empty).Trim();
            request.definicion.notas = (request.definicion.notas ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.definicion.cod_plazo_bono))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del plan de bonificacion."
                };
            }

            if (string.IsNullOrWhiteSpace(request.definicion.descripcion))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la descripcion del plan."
                };
            }

            if (string.IsNullOrWhiteSpace(request.definicion.notas))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar las notas del plan."
                };
            }

            if (!request.editar)
            {
                if (ExistePlan(codEmpresa, request.definicion.cod_plazo_bono))
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El codigo del plan ya existe."
                    };
                }

                return InsertarPlan(codEmpresa, request);
            }

            if (!string.Equals(
                request.codigo_original,
                request.definicion.cod_plazo_bono,
                StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Ha modificado el codigo del plan."
                };
            }

            return ActualizarPlan(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un plan de bonificación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPlazosBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_plazo_bono = Limpiar(request.cod_plazo_bono);

            if (string.IsNullOrWhiteSpace(request.cod_plazo_bono))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del plan de bonificacion."
                };
            }

            const string sqlDeleteAsignaciones = @"
                delete from CRD_PLAZO_BONO_ASG
                where cod_Plazo_Bono = @CodPlazoBono;";

            const string sqlDeleteBonificaciones = @"
                delete from CRD_PLAZO_BONO_MEMBRESIA
                where cod_Plazo_Bono = @CodPlazoBono;";

            const string sqlDeletePlan = @"
                delete from CRD_PLAZO_BONO
                where cod_Plazo_Bono = @CodPlazoBono;";

            var respAsignaciones = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteAsignaciones,
                new { CodPlazoBono = request.cod_plazo_bono });

            if (respAsignaciones.Code < 0)
            {
                return respAsignaciones;
            }

            var respBonificaciones = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteBonificaciones,
                new { CodPlazoBono = request.cod_plazo_bono });

            if (respBonificaciones.Code < 0)
            {
                return respBonificaciones;
            }

            var respPlan = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeletePlan,
                new { CodPlazoBono = request.cod_plazo_bono });

            if (respPlan.Code < 0)
            {
                return respPlan;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Plazos: Plan de Bonificacion : {request.cod_plazo_bono}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = EliminadoExitoso
            };
        }

        /// <summary>
        /// Obtiene las líneas de bonificación de un plan.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlazoBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPlazosBonificacionBonificacionData>> CrPlazosBonificacion_Bonificaciones_Obtener(
            int codEmpresa,
            string codPlazoBono)
        {
            codPlazoBono = Limpiar(codPlazoBono);

            const string sql = @"
                select
                    isnull(Linea, 0) as linea,
                    isnull(Inicio, 0) as inicio,
                    isnull(Corte, 0) as corte,
                    isnull(Plazo, 0) as plazo,
                    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
                    Registro_Fecha as registro_fecha
                from CRD_PLAZO_BONO_MEMBRESIA
                where cod_Plazo_Bono = @CodPlazoBono
                order by Linea;";

            return DbHelper.ExecuteListQuery<CrPlazosBonificacionBonificacionData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlazoBono = codPlazoBono
                }
            );
        }

        /// <summary>
        /// Guarda una línea de bonificación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPlazosBonificacion_Bonificaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_plazo_bono = Limpiar(request.cod_plazo_bono);

            if (string.IsNullOrWhiteSpace(request.cod_plazo_bono))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del plan de bonificacion."
                };
            }

            if (request.bonificacion.inicio <= 0 || request.bonificacion.corte <= 0 || request.bonificacion.plazo <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar valores validos para inicio, corte y plazo."
                };
            }

            if (request.bonificacion.linea <= 0)
            {
                return InsertarBonificacion(codEmpresa, request);
            }

            return ActualizarBonificacion(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una línea de bonificación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPlazosBonificacion_Bonificaciones_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_plazo_bono = Limpiar(request.cod_plazo_bono);

            if (string.IsNullOrWhiteSpace(request.cod_plazo_bono) || request.linea <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el plan y la linea a eliminar."
                };
            }

            const string sql = @"
                delete from CRD_PLAZO_BONO_MEMBRESIA
                where cod_Plazo_Bono = @CodPlazoBono
                  and Linea = @Linea;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlazoBono = request.cod_plazo_bono,
                    Linea = request.linea
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Tasas Bonfificacion: P:{request.cod_plazo_bono}..L: {request.linea}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = EliminadoExitoso
            };
        }

        /// <summary>
        /// Obtiene las garantías asignables y asignadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlazoBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPlazosBonificacionAsignacionData>> CrPlazosBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string codPlazoBono)
        {
            codPlazoBono = Limpiar(codPlazoBono);

            const string sql = @"
                select
                    rtrim(isnull(G.garantia, '')) as garantia,
                    rtrim(isnull(G.descripcion, '')) as descripcion,
                    cast(case when A.registro_fecha is null then 0 else 1 end as bit) as asignado
                from CRD_GARANTIA_TIPOS G
                left join CRD_PLAZO_BONO_ASG A
                    on G.garantia = A.GARANTIA
                   and A.cod_Plazo_Bono = @CodPlazoBono
                order by G.descripcion;";

            return DbHelper.ExecuteListQuery<CrPlazosBonificacionAsignacionData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlazoBono = codPlazoBono
                }
            );
        }

        /// <summary>
        /// Guarda o elimina una asignación de garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPlazosBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionAsignacionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_plazo_bono = Limpiar(request.cod_plazo_bono);
            request.garantia = Limpiar(request.garantia);

            if (string.IsNullOrWhiteSpace(request.cod_plazo_bono) || string.IsNullOrWhiteSpace(request.garantia))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el plan y la garantia."
                };
            }

            if (request.asignado)
            {
                if (!ExisteAsignacion(codEmpresa, request.cod_plazo_bono, request.garantia))
                {
                    const string sqlInsert = @"
                        insert into CRD_PLAZO_BONO_ASG
                        (
                            cod_Plazo_Bono,
                            garantia,
                            registro_fecha,
                            registro_usuario
                        )
                        values
                        (
                            @CodPlazoBono,
                            @Garantia,
                            dbo.MyGetdate(),
                            @Usuario
                        );";

                    var respInsert = DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        sqlInsert,
                        new
                        {
                            CodPlazoBono = request.cod_plazo_bono,
                            Garantia = request.garantia,
                            Usuario = request.usuario
                        }
                    );

                    if (respInsert.Code < 0)
                    {
                        return respInsert;
                    }
                }
            }
            else
            {
                const string sqlDelete = @"
                    delete from CRD_PLAZO_BONO_ASG
                    where cod_Plazo_Bono = @CodPlazoBono
                      and Garantia = @Garantia;";

                var respDelete = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlDelete,
                    new
                    {
                        CodPlazoBono = request.cod_plazo_bono,
                        Garantia = request.garantia
                    }
                );

                if (respDelete.Code < 0)
                {
                    return respDelete;
                }
            }

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto InsertarPlan(int codEmpresa, CrPlazosBonificacionDefinicionGuardarRequest request)
        {
            const string sql = @"
                insert into CRD_PLAZO_BONO
                (
                    cod_Plazo_Bono,
                    descripcion,
                    Notas,
                    Activo,
                    Registro_Fecha,
                    Registro_Usuario
                )
                values
                (
                    @CodPlazoBono,
                    @Descripcion,
                    @Notas,
                    @Activo,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlazoBono = request.definicion.cod_plazo_bono,
                    Descripcion = request.definicion.descripcion,
                    Notas = request.definicion.notas,
                    Activo = request.definicion.activo ? 1 : 0,
                    Usuario = request.usuario
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Plazos: Plan de Bonificacion : {request.definicion.cod_plazo_bono}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto ActualizarPlan(int codEmpresa, CrPlazosBonificacionDefinicionGuardarRequest request)
        {
            const string sql = @"
                update CRD_PLAZO_BONO
                   set descripcion = @Descripcion,
                       Notas = @Notas,
                       Activo = @Activo
                 where cod_Plazo_Bono = @CodPlazoBono;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlazoBono = request.definicion.cod_plazo_bono,
                    Descripcion = request.definicion.descripcion,
                    Notas = request.definicion.notas,
                    Activo = request.definicion.activo ? 1 : 0
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Plazos: Plan de Bonificacion : {request.codigo_original}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto InsertarBonificacion(int codEmpresa, CrPlazosBonificacionBonificacionGuardarRequest request)
        {
            const string sqlLinea = @"
                select isnull(max(Linea), 0) + 1
                from CRD_PLAZO_BONO_MEMBRESIA
                where cod_Plazo_Bono = @CodPlazoBono;";

            var respLinea = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlLinea,
                1,
                new
                {
                    CodPlazoBono = request.cod_plazo_bono
                }
            );

            if (respLinea.Code < 0)
            {
                return new ErrorDto
                {
                    Code = respLinea.Code,
                    Description = respLinea.Description
                };
            }

            int nuevaLinea = respLinea.Result;

            const string sqlInsert = @"
                insert into CRD_PLAZO_BONO_MEMBRESIA
                (
                    cod_Plazo_Bono,
                    Linea,
                    Inicio,
                    Corte,
                    Plazo,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @CodPlazoBono,
                    @Linea,
                    @Inicio,
                    @Corte,
                    @Plazo,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodPlazoBono = request.cod_plazo_bono,
                    Linea = nuevaLinea,
                    Inicio = request.bonificacion.inicio,
                    Corte = request.bonificacion.corte,
                    Plazo = request.bonificacion.plazo,
                    Usuario = request.usuario
                }
            );

            if (respInsert.Code < 0)
            {
                return respInsert;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Tasas Bonfificacion: P:{request.cod_plazo_bono}..L: {nuevaLinea}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto ActualizarBonificacion(int codEmpresa, CrPlazosBonificacionBonificacionGuardarRequest request)
        {
            const string sqlUpdate = @"
                update CRD_PLAZO_BONO_MEMBRESIA
                   set Modifica_Fecha = dbo.MyGetdate(),
                       Modifica_Usuario = @Usuario,
                       Inicio = @Inicio,
                       Corte = @Corte,
                       Plazo = @Plazo
                 where cod_Plazo_Bono = @CodPlazoBono
                   and Linea = @Linea;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodPlazoBono = request.cod_plazo_bono,
                    Linea = request.bonificacion.linea,
                    Inicio = request.bonificacion.inicio,
                    Corte = request.bonificacion.corte,
                    Plazo = request.bonificacion.plazo,
                    Usuario = request.usuario
                }
            );

            if (respUpdate.Code < 0)
            {
                return respUpdate;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Tasas Bonfificacion: P:{request.cod_plazo_bono}..L: {request.bonificacion.linea}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private bool ExistePlan(int codEmpresa, string codPlazoBono)
        {
            const string sql = @"
                select coalesce(count(*), 0)
                from CRD_PLAZO_BONO
                where cod_Plazo_Bono = @CodPlazoBono;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodPlazoBono = codPlazoBono
                }
            );

            return resp.Result > 0;
        }

        private bool ExisteAsignacion(int codEmpresa, string codPlazoBono, string garantia)
        {
            const string sql = @"
                select coalesce(count(*), 0)
                from CRD_PLAZO_BONO_ASG
                where cod_Plazo_Bono = @CodPlazoBono
                  and Garantia = @Garantia;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodPlazoBono = codPlazoBono,
                    Garantia = garantia
                }
            );

            return resp.Result > 0;
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

        private static string Limpiar(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}