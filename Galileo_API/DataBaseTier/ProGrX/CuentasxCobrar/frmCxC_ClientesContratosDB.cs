using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data.Common;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCClientesContratosModels;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesContratosDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCxC = 31;
        private const string MovActualiza = "MODIFICA - WEB";
        private const string MovRegistra = "Registra - WEB";
        private const string MovEliminar = "ELIMINAR - WEB";
        private static ErrorDto Err(string msg) => DbHelper.ErrorResponse(msg);

        public FrmCxCClientesContratosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);

        }
        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        /// <summary>
        ///  Consulta los datos generales de un contrato específico para un cliente por orden 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="orden"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<string> CxCClientesPersonas_Contratos_Consultar(int codEmpresa, string cedula, int orden, string contrato)
        {
            string query = $@"select Top 1 cod_contrato from CxC_Personas_Contratos ";

            if (orden == 1)
            {
                query += $"where cod_contrato > @contrato   and cedula = @cedula  order by cod_contrato asc";
            }
            else
            {
                query += $"where cod_contrato > @contrato   and cedula = @cedula  order by cod_contrato desc";
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {

                return conn.QueryFirstOrDefault<string>(query, new { cedula, contrato }) ?? "";
            });
        }

        /// <summary>
        ///  Consulta los datos generales de un contrato específico para un cliente
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<ClientesContratosData> CxCContratos_Consultar(int codEmpresa, string cedula, string contrato)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            var respuesta = new ErrorDto<ClientesContratosData>
            {
                Code = 0,
                Description = "",
                Result = new ClientesContratosData()
            };


            string query = $@"select P.Descripcion,C.*
                        from CxC_Contratos P inner join CxC_Personas_Contratos C on P.cod_contrato = C.cod_contrato
                        where C.cedula = @cedula  and C.cod_contrato =@contrato ";


            var respNullable = DbHelper.ExecuteSingleQuery<ClientesContratosData?>(
                 _portalDB,
                 codEmpresa,
                 query,
                 new ClientesContratosData(),
                 new { cedula, contrato });


            if (respNullable.Code == 0 && respNullable.Result == null)
            {
                string query2 = $@"select *,dbo.MyGetdate() as 'Fecha' from CxC_Contratos where cod_contrato = @contrato  and activo = 1";
                var respNullable2 = DbHelper.ExecuteSingleQuery<ClientesContratosData?>(
                      _portalDB,
                      codEmpresa,
                      query2,
                      new ClientesContratosData(),
                      new { cedula, contrato });

                respuesta.Code = respNullable2.Code;
                respuesta.Description = respNullable2.Description;
                respuesta.Result = respNullable2.Result ?? new ClientesContratosData();

            }
            else
            {
                respuesta.Code = respNullable.Code;
                respuesta.Description = respNullable.Description;
                respuesta.Result = respNullable.Result ?? new ClientesContratosData();

            }

            return respuesta;

        }

        /// <summary>
        /// Inserta o actualiza los datos generales de un contrato para una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonasContratos_Guardar(int codEmpresa, string usuario, ClientesContratosData datos)
        {
            if (datos is null) return Err("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Cedula)) return Err("El campo 'cedula' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Err("El usuario es requerido.");

            try
            {
                return datos.IsNew
                     ? CxCClientesPersonas_Contratos_Insertar(codEmpresa, usuario, datos)
                     : CxCClientesPersonas_Contratos_Actualizar(codEmpresa, usuario, datos);
 

            }
            catch (DbException)
            {
                return Err("No fue posible guardar.");
            }
            catch (Exception ex)
            {
                return Err("Error inesperado al guardar: " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo contrato  para una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCClientesPersonas_Contratos_Insertar(int CodEmpresa, string usuario, ClientesContratosData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                           insert CxC_Personas_Contratos(cedula,cod_contrato,Notas,Activo,Plazo,Tasa_Corriente,Tasa_Mora,registro_usuario,registro_fecha,
                                Contrato_Num,Contrato_Tipo,Contrato_Vence)
                            VALUES (
                                @cedula, @Cod_Contrato, @Notas, @Activo,@Plazo,@Tasa_Corriente,@Tasa_Mora,@usuario,dbo.MyGetdate(),@Contrato_Num,@Contrato_Tipo,@Contrato_Vence)";

                conn.Execute(query, new
                {
                    datos.Cedula,
                    datos.Cod_Contrato,
                    datos.Notas,
                    datos.Activo,
                    datos.Plazo,
                    datos.Tasa_Corriente,
                    datos.Tasa_Mora,
                    usuario,
                    datos.Contrato_Num,
                    datos.Contrato_Tipo,
                    datos.Contrato_Vence
                });

                var detalle = $"Suscripción: Ced: {datos.Cedula} Cnt:{datos.Cod_Contrato}";

                LogBitacora(CodEmpresa, usuario, detalle, MovRegistra);


                return DbHelper.OkResponse("datos insertados correctamente.");
            }

            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Modifica un registro de contrato existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CxCClientesPersonas_Contratos_Actualizar(int CodEmpresa, string usuario, ClientesContratosData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            UPDATE CxC_Personas_Contratos
                            SET 
                                Cedula = @cedula,
                                Notas = @Notas,
                                Activo = @Activo,
                                Plazo = @Plazo,
                                Tasa_Corriente= @Tasa_Corriente,
                                Tasa_Mora = @Tasa_Mora,
                                Actualiza_Usuario = @usuario ,
                                Actualiza_Fecha = dbo.MyGetdate(),
                                Contrato_Num = @Contrato_Num ,
                                Contrato_Tipo = @Contrato_Tipo,
                                Contrato_Vence = @Contrato_Vence                                
                            WHERE cedula = @cedula  and Cod_Contrato =@Cod_Contrato";

                conn.Execute(query, new
                {
                    datos.Cedula,
                    datos.Cod_Contrato,
                    datos.Notas,
                    datos.Activo,
                    datos.Plazo,
                    datos.Tasa_Corriente,
                    datos.Tasa_Mora,
                    usuario,
                    datos.Contrato_Num,
                    datos.Contrato_Tipo,
                    datos.Contrato_Vence
                });

                var detalle = $"Suscripción: Ced: {datos.Cedula} Cnt:{datos.Cod_Contrato}";

                LogBitacora(CodEmpresa, usuario, detalle, MovActualiza);

                return DbHelper.OkResponse("Datos actualizado correctamente.");
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Elimina un contrato específico de un cliente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonas_Contratos_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete CxC_Personas_Contratos where cedula =@cedula  and cod_contrato =@contrato ";
                conn.Execute(query, new { cedula, contrato });
                var detalle = $"Suscripción: Ced: {cedula} Cnt:{contrato}";

                LogBitacora(CodEmpresa, usuario, detalle, MovEliminar);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Elimina la suscripcion de un contrato de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <param name="cargo"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cargo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete CxC_Personas_Contratos_Suscripciones where cedula =@cedula  and cod_contrato =@contrato and cod_cargo =@cargo";
                conn.Execute(query, new { cedula, contrato, cargo });

                var detalle = $"Suscripción: Ced: {cedula} Cnt:{contrato} Cargo.: {cargo}";

                LogBitacora(CodEmpresa, usuario, detalle, MovEliminar);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

        /// <summary>
        ///  Elimina la suscripcion de un contrato de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Insertar(int CodEmpresa, string usuario, PersonasContratosSuscripcionesData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                           insert CxC_Personas_Contratos_Suscripciones(
                                cod_contrato,cedula,cod_cargo,Tipo,valor,frecuencia_Tipo,frecuencia_dias,recaudado,pago_ultimo,pago_proximo,modifica,registro_Fecha,Registro_Usuario)
                            VALUES (
                                @Cod_Contrato, @cedula, @Cod_cargo, @Tipo,@Valor,@Frecuencia_tipo,@Frecuencia_dias,@Recaudado,@Pago_ultimo,@Pago_proximo,@Modifica,dbo.MyGetdate(),@usuario )";

                conn.Execute(query, new
                {
                    datos.Cod_Contrato,
                    datos.Cedula,
                    datos.Cod_cargo,
                    datos.Tipo,
                    datos.Valor,
                    datos.Frecuencia_tipo,
                    datos.Frecuencia_dias,
                    datos.Recaudado,
                    datos.Pago_ultimo,
                    datos.Pago_proximo,
                    datos.Modifica,
                    usuario
                });

                var detalle = $"Suscripción: Ced: {datos.Cedula} Cnt:{datos.Cod_Contrato} Cargo.: {datos.Cod_cargo}";

                LogBitacora(CodEmpresa, usuario, detalle, MovRegistra);


                return DbHelper.OkResponse("Persona contrato suscripcion correctamente.");
            }

            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        ///  Elimina el pagador de un contrato de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <param name="cedula_pagador"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cedula_pagador)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"delete CxC_Personas_Contratos_Pagadores where cedula =@cedula  and cod_contrato =@contrato and cedula_pagador =@cedula_pagador";
                conn.Execute(query, new { cedula, contrato, cedula_pagador });

                var detalle = $"Suscripción: Ced: {cedula} Cnt:{contrato} Pagador.: {cedula_pagador}";

                LogBitacora(CodEmpresa, usuario, detalle, MovEliminar);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

        /// <summary>
        ///  Inserta el pagador de un contrato de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Insertar(int CodEmpresa, string usuario, PersonasContratosPagadoresData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                           insert CxC_Personas_Contratos_Pagadores(
                            cod_contrato,cedula,cedula_pagador,registro_fecha,registro_usuario)
                            VALUES (
                                @Cod_Contrato, @cedula, @Cedula_pagador,dbo.MyGetdate(),@usuario )";

                conn.Execute(query, new
                {
                    datos.Cod_Contrato,
                    datos.Cedula,
                    datos.Cedula_pagador,
                    usuario
                });

                var detalle = $"Suscripción: Ced: {datos.Cedula} Cnt:{datos.Cod_Contrato} Pagador.: {datos.Cedula_pagador}";

                LogBitacora(CodEmpresa, usuario, detalle, MovRegistra);


                return DbHelper.OkResponse("Persona contrato pagador insertado correctamente.");
            }

            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        ///  Consulta la lista de pagadores asociados a un contrato específico para un cliente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<PersonasContratosPagadoresData>> CxCClientesPersonas_ContratosPagadores_Lista(int CodEmpresa, string cedula, string contrato)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@" 
                              
                 select P.nombre, C.COD_CONTRATO,C.CEDULA,C.CEDULA_PAGADOR, CONCAT(CONVERT(varchar(10), C.REGISTRO_FECHA, 103), '...', C.REGISTRO_USUARIO) AS registro
                    from CxC_Personas P 
                     inner join CxC_Personas_Contratos_Pagadores C on P.cedula = C.cedula_pagador 
                     where C.cod_contrato = @contrato and C.cedula = @cedula
                 UNION
                 select P.nombre, C.COD_CONTRATO,C.CEDULA,C.CEDULA AS CEDULA_PAGADOR,'' AS registro
                     from CxC_Personas P inner join CxC_Contratos_Pagadores C on P.cedula = C.cedula
                     where C.cod_contrato =@contrato
                     and C.cedula <> @cedula
                     and C.cedula not in(
                     select Cedula_Pagador from CxC_Personas_Contratos_Pagadores
                      Where Cedula = @cedula
                      and Cod_Contrato =@contrato)   ";

                return conn.Query<PersonasContratosPagadoresData>(query, new { cedula, contrato }).ToList();
            });
        }


        /// <summary>
        ///  Consulta la lista de sSuscripciones asociados a un contrato específico para un cliente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<PersonasContratosSuscripcionesData>> CxCClientesPersonas_ContratosSuscripcion_Lista(int CodEmpresa, string cedula, string contrato)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"   select S.COD_CARGO, C.descripcion,S.Tipo,S.Frecuencia_Tipo,S.Valor,S.Frecuencia_dias,S.Recaudado,S.Pago_Ultimo,S.Pago_Proximo,S.Modifica
                                    from CxC_Cargos C inner join CxC_Personas_Contratos_Suscripciones S on C.cod_cargo = S.cod_cargo
                                    where S.cod_contrato = @contrato
                                    and S.cedula =@cedula
                                     UNION
                                     select 
                                     S.COD_CARGO, C.descripcion,S.Tipo,S.Frecuencia_Tipo,S.Valor,S.Frecuencia_dias, 0 as 'Recaudado',dbo.MyGetdate() as 'Pago_Ultimo',dateadd(d,S.frecuencia_dias,dbo.MyGetdate()) as 'Pago_Proximo',S.Modifica
                                         from CxC_Cargos C inner join CxC_Contratos_Cargos S on C.cod_cargo = S.cod_cargo
                                        where S.cod_contrato =@contrato
                                        and S.cod_cargo not in(select cod_cargo from CxC_Personas_Contratos_Suscripciones
                                        where cod_contrato = @contrato
                                        and cedula =@cedula)";

                return conn.Query<PersonasContratosSuscripcionesData>(query, new { cedula, contrato }).ToList();
            });
        }
       
        /// <summary>
        /// Consulta el listado de contratos activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Contratos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select cod_contrato as item,descripcion as descripcion 
                        from CxC_Contratos where activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query, new { ModuloCxC }).ToList();
            });
        }

    }
}