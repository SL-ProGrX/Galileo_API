using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_AprDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;
       
        public frmAF_Beneficios_Integral_AprDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);     
        }

        /// <summary>
        /// Cargo las categorias de apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> CategoriaAPT_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select ID_APT_CATEGORIA as 'item', DESCRIPCION as 'descripcion' from AFI_BENE_APT_CATEGORIAS " +
                        " where Activo = 1  order by Descripcion asc ";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Cargo los profesionales de apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProfecionalAPT_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select ID_PROFESIONAL as 'item', NOMBRE as 'descripcion' from AFI_BENE_APT_PROFESIONALES " +
                        " where Activo = 1  order by NOMBRE asc ";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #region Nucleo Familiar
        /// <summary>
        /// Guardo el miembro del nucleo familiar
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="miembro"></param>
        /// <returns></returns>
        public ErrorDto MiembroFamiliar_Guardar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {

                //segundo filtro de validaciones
                var respValida = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, miembro.cedula.Trim());
                if (respValida.Code == -1)
                {
                    info.Code = -1;
                    info.Description = respValida.Description;
                    return info;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el miembro ya existe
                    if (miembro.id_socio_familia != 0)
                    {

                        ErrorDto<bool> response = MiembroFamiliar_Actualizar(CodCliente, miembro);

                        if (!response.Result)
                        {
                            info.Code = -1;
                            info.Description = response.Description;
                        }
                        else
                        {
                            info.Description = miembro.id_socio_familia.ToString();
                        }
                    }
                    else
                    {
                        info = MiembroFamiliar_Agregar(CodCliente, miembro);
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Agrego el miembro del nucleo familiar
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="miembro"></param>
        /// <returns></returns>
        public ErrorDto MiembroFamiliar_Agregar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //valido si existe cedula
                    var query = @$"SELECT COUNT(*) FROM [dbo].[AFI_BENE_SOCIO_FAMILIA] WHERE CEDULA_PARIENTE = '{miembro.cedula_pariente}' AND CEDULA = '{miembro.cedula.Trim()}' ";
                    var count = connection.Query<int>(query).FirstOrDefault();

                    if (count > 0)
                    {
                        info.Code = -1;
                        info.Description = "Ya existe un miembro con la cedula ingresada";
                        return info;
                    }

                    int becado = miembro.estudiante_becado ? 1 : 0;

                    query = @$"INSERT INTO [dbo].[AFI_BENE_SOCIO_FAMILIA]
                                           ([CEDULA]
                                           ,[PARENTESCO]
                                           ,[APELLIDO_1]
                                           ,[APELLIDO_2]
                                           ,[NOMBRE]
                                           ,[NACIONALIDAD]
                                           ,[CEDULA_PARIENTE]
                                           ,[EDAD]
                                           ,[ESTADO_CIVIL]
                                           ,[ACTIVIDAD_REALIZA]
                                           ,[OCUPACION]
                                           ,[DESEMPLEO]
                                           ,[CONDICION_ASEGURAMIENTO]
                                           ,[INGRESO_BRUTO]
                                           ,[PENSION_TIPO]
                                           ,[DISCAPACIDAD_TIPO]
                                           ,[DISCAPACIDAD_DESC]
                                           ,[CENTRO_EDUCATIVO]
                                           ,[GRADO_ACADEMICO]
                                           ,[ESTUDIANTE_BECADO]
                                           ,[EJERCE_CUIDO]
                                           ,[PAGO_X_CUIDO]
                                           ,[OBSERVACIONES]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO]
                                           ,[ACTIVO])
                                     VALUES
                                           ('{miembro.cedula}'
                                           ,'{((miembro.parentesco == null) ? "" : miembro.parentesco.item)}'
                                           ,'{miembro.apellido_1}'
                                           ,'{miembro.apellido_2}'
                                           ,'{miembro.nombre}'
                                           ,'{((miembro.nacionalidad == null) ? "" : miembro.nacionalidad.item)}'
                                           ,'{miembro.cedula_pariente}'
                                           ,{miembro.edad}
                                           ,'{((miembro.estado_civil == null) ? "" : miembro.estado_civil.item)}'
                                           ,'{((miembro.actividad_realiza == null) ? "" : miembro.actividad_realiza.item)}'
                                           ,'{miembro.ocupacion}'
                                           ,'{((miembro.desempleo == null) ? "" : miembro.desempleo.item)}'
                                           ,'{((miembro.condicion_aseguramiento == null) ? "" : miembro.condicion_aseguramiento.item)}'
                                           ,{miembro.ingreso_bruto}
                                           ,'{((miembro.pension_tipo == null) ? "" : miembro.pension_tipo.item)}'
                                           ,'{((miembro.discapacidad_tipo == null) ? "" : miembro.discapacidad_tipo.item)}'
                                           ,'{miembro.discapacidad_desc}'
                                           ,'{miembro.centro_educativo}'
                                           ,'{((miembro.grado_academico == null) ? "" : miembro.grado_academico.item)}'
                                           ,{becado}
                                           ,'{((miembro.ejerce_cuido == null) ? "" : miembro.ejerce_cuido.item)}'
                                           ,{miembro.pago_x_cuido}
                                           ,'{miembro.observaciones}'
                                           ,getDate()
                                           ,'{miembro.registro_usuario}'
                                           ,1)
                    ";
                    info.Code = connection.Execute(query);

                    query = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_FAMILIA') as 'id'";
                    var id = connection.Query<int>(query).FirstOrDefault();

                    info.Description = id.ToString();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Actualizo el miembro del nucleo familiar
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="miembro"></param>
        /// <returns></returns>
        private ErrorDto<bool> MiembroFamiliar_Actualizar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<bool>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = miembro.activo ? 1 : 0;
                    int becado = miembro.estudiante_becado ? 1 : 0;

                    var query = @$"UPDATE [dbo].[AFI_BENE_SOCIO_FAMILIA]
                                   SET 
                                       [PARENTESCO] = {miembro.parentesco.item}
                                      ,[APELLIDO_1] = '{miembro.apellido_1}'
                                      ,[APELLIDO_2] = '{miembro.apellido_2}'
                                      ,[NOMBRE] = '{miembro.nombre}'
                                      ,[NACIONALIDAD] = '{((miembro.nacionalidad == null) ? "" : miembro.nacionalidad.item)}'
                                      ,[CEDULA_PARIENTE] = '{miembro.cedula_pariente}'
                                      ,[EDAD] = {miembro.edad}
                                      ,[ESTADO_CIVIL] = '{((miembro.estado_civil == null) ? "" : miembro.estado_civil.item)}'
                                      ,[ACTIVIDAD_REALIZA] = '{((miembro.actividad_realiza == null) ? "" : miembro.actividad_realiza.item)}'
                                      ,[OCUPACION] = '{miembro.ocupacion}'
                                      ,[DESEMPLEO] = '{((miembro.desempleo == null) ? "" : miembro.desempleo.item)}'
                                      ,[CONDICION_ASEGURAMIENTO] = '{((miembro.condicion_aseguramiento == null) ? "" : miembro.condicion_aseguramiento.item)}'
                                      ,[INGRESO_BRUTO] = {miembro.ingreso_bruto}
                                      ,[PENSION_TIPO] = '{((miembro.pension_tipo == null) ? "" : miembro.pension_tipo.item)}'
                                      ,[DISCAPACIDAD_TIPO] = '{((miembro.discapacidad_tipo == null) ? "" : miembro.discapacidad_tipo.item)}'
                                      ,[DISCAPACIDAD_DESC] = '{miembro.discapacidad_desc}'
                                      ,[CENTRO_EDUCATIVO] = '{miembro.centro_educativo}'
                                      ,[GRADO_ACADEMICO] = '{((miembro.grado_academico == null) ? "" : miembro.grado_academico.item)}'
                                      ,[ESTUDIANTE_BECADO] = {becado}
                                      ,[EJERCE_CUIDO] = '{((miembro.ejerce_cuido == null) ? "" : miembro.ejerce_cuido.item)}'
                                      ,[PAGO_X_CUIDO] = {miembro.pago_x_cuido}
                                      ,[OBSERVACIONES] = '{miembro.observaciones}'
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{miembro.modifica_usuario}'
                                      ,[ACTIVO] = {activo}
                                 WHERE ID_SOCIO_FAMILIA = {miembro.id_socio_familia} ";


                    response.Code = connection.Execute(query);
                    response.Result = true;

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = false;
                response.Description = ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Obtengo los miembros del nucleo familiar por cedula de socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<BeneIntNucleoFamLista>> MiembrosFamiliar_Obtener(int CodCliente, string? cedula)
        {

           

            var response = new ErrorDto<List<BeneIntNucleoFamLista>>();

            if (cedula == null)
            {
                return response;
            }

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (cedula == null)
                    {
                        response.Code = -1;
                        response.Description = "Cedula no puede ser nula";
                        response.Result = null;
                    }
                    var query = "select * from AFI_BENE_SOCIO_FAMILIA where CEDULA = '" + cedula.Trim() + "' and ACTIVO = 1";
                    response.Result = connection.Query<BeneIntNucleoFamLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
        /// <summary>
        /// Elimino un miembro del nucleo familiar
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto MiembroFamiliar_Eliminar(int CodCliente, long id, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"UPDATE [dbo].[AFI_BENE_SOCIO_FAMILIA]
                                   SET 
                                       [ACTIVO] = 0
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{usuario}'
                                 WHERE ID_SOCIO_FAMILIA = {id} ";

                    info.Code = connection.Execute(query);
                    info.Description = "Miembro familiar eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Situacion Financiera
        /// <summary>
        /// Guardo datos de situacion financiera del socio desde categoria de apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="finanza"></param>
        /// <returns></returns>
        public ErrorDto SituacionFinanciera_Guardar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el tipo de situacion financiera es manutencion
                    if (finanza.tipo != "M")
                    {
                        if (finanza.id != 0)
                        {
                            ErrorDto<bool> response = SituacionFinanciera_Actualizar(CodCliente, finanza);
                            if (!response.Result)
                            {
                                info.Code = -1;
                                info.Description = response.Description;
                            }
                            else
                            {
                                info.Description = finanza.id.ToString();
                            }
                        }
                        else
                        {
                            info = SituacionFinanciera_Agregar(CodCliente, finanza);
                        }
                    }
                    else
                    {
                        ErrorDto<bool> response = Manutencion_Guardar(CodCliente, finanza);
                        if (!response.Result)
                        {
                            info.Code = -1;
                            info.Description = response.Description;
                        }
                        else
                        {
                            info.Description = finanza.id.ToString();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Inserto un nuevo registro de situacion financiera
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="finanza"></param>
        /// <returns></returns>
        private ErrorDto SituacionFinanciera_Agregar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = @$"INSERT INTO [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                           ([CEDULA]
                                           ,[TIPO]
                                           ,[ID_CONCEPTO]
                                           ,[CONCEPTO]
                                           ,[MONTO]
                                           ,[OBSERVACIONES]
                                           ,[ACREEDOR]
                                           ,[DEUDOR]
                                           ,[CUOTA]
                                           ,[SALDO]
                                           ,[MOROSIDAD]
                                           ,[REGISTRA_USUARIO]
                                           ,[REGISTRA_FECHA]
                                           ,[ACTIVO])
                                     VALUES
                                           ('{finanza.cedula.Trim()}'
                                           ,'{finanza.tipo}'
                                           ,'{finanza.id_concepto.item}'
                                           ,'{finanza.concepto}'
                                           ,{finanza.monto}
                                           ,'{finanza.observaciones}'
                                            ,'{finanza.acreedor}'
                                            ,'{finanza.deudor}'
                                            ,{finanza.cuota}
                                            ,{finanza.saldo}
                                            ,{finanza.morosidad}
                                           ,'{finanza.registra_Usuario}'
                                           ,getDate()
                                           ,1)";
                    info.Code = connection.Execute(query);

                    query = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_FINANZAS') as 'id'";
                    finanza.id = connection.Query<int>(query).FirstOrDefault();

                    info.Description = finanza.id.ToString();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Actualizo un registro de situacion financiera
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="finanza"></param>
        /// <returns></returns>
        private ErrorDto<bool> SituacionFinanciera_Actualizar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<bool>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = finanza.activo ? 1 : 0;

                    var query = @$"UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                               SET
                                   [ID_CONCEPTO] = '{finanza.id_concepto.item}'
                                  ,[CONCEPTO] = '{finanza.concepto}'
                                  ,[MONTO] = {finanza.monto}
                                  ,[OBSERVACIONES] = '{finanza.observaciones}'
                                  ,[ACREEDOR] = '{finanza.acreedor}'
                                  ,[DEUDOR] = '{finanza.deudor}'
                                  ,[CUOTA] = {finanza.cuota}
                                  ,[SALDO] = {finanza.saldo}
                                  ,[MOROSIDAD] = {finanza.morosidad}
                                  ,[MODIFICA_USUARIO] = '{finanza.modifica_Usuario}'
                                  ,[MODIFICA_FECHA] = getDate()
                                  ,[ACTIVO] = {activo}
                             WHERE ID_SITUACIONFINANCIERA = {finanza.id} ";

                    response.Code = connection.Execute(query);
                    response.Result = true;

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = false;
                response.Description = ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Obtengo los datos de situacion financiera del socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneSocioFinanzas>> SituacionFinSocio_Obtener(int CodCliente, string? cedula, string tipo)
        {
            var response = new ErrorDto<List<AfiBeneSocioFinanzas>>();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string where = "";
                    if (cedula != null)
                    {
                        where = @$" TRIM(CEDULA) = '{cedula.Trim()}' and ";
                    }

                    var query = @$"select ID_SITUACIONFINANCIERA as id, ID_CONCEPTO, CEDULA, TIPO ,CONCEPTO, MONTO , OBSERVACIONES, ACTIVO
                                              ,ACREEDOR
                                              ,DEUDOR
                                              ,CUOTA
                                              ,SALDO
                                              ,MOROSIDAD from AFI_BENE_SOCIO_FINANZAS  
                                    where {where} TIPO = '{tipo}' and ACTIVO = 1";
                    response.Result = connection.Query<AfiBeneSocioFinanzas>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
        /// <summary>
        /// Inactiva un registro de situacion financiera
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto SituacionFinanciera_Eliminar(int CodCliente, int id, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                   SET 
                                       [ACTIVO] = 0
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{usuario}'
                                 WHERE ID_SITUACIONFINANCIERA = {id} ";

                    info.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Actualizo registro de manutencion del socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="finanza"></param>
        /// <returns></returns>
        private ErrorDto<bool> Manutencion_Guardar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<bool>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //pregunto si cedula ya tiene manutencion
                    var query = @$"SELECT COUNT(*) FROM [dbo].[AFI_BENE_SOCIO_FINANZAS] WHERE CEDULA = '{finanza.cedula.Trim()}' AND TIPO = 'M' ";
                    var count = connection.Query<int>(query).FirstOrDefault();

                    if (count > 0)
                    {
                        query = @$"UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                   SET 
                                      [MONTO] = {finanza.monto}
                                      ,[MODIFICA_USUARIO] = '{finanza.modifica_Usuario}'
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[ACTIVO] = 1
                                 WHERE CEDULA = '{finanza.cedula.Trim()}' AND TIPO = 'M' ";
                    }
                    else
                    {
                        query = @$"INSERT INTO [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                           ([CEDULA]
                                           ,[TIPO]
                                           ,[ID_CONCEPTO]
                                           ,[CONCEPTO]
                                           ,[MONTO]
                                           ,[OBSERVACIONES]
                                           ,[REGISTRA_USUARIO]
                                           ,[REGISTRA_FECHA]
                                           ,[ACTIVO])
                                     VALUES
                                           ('{finanza.cedula.Trim()}'
                                           ,'{finanza.tipo}'
                                           ,'{finanza.id_concepto.item}'
                                           ,'{finanza.concepto}'
                                           ,{finanza.monto}
                                           ,'{finanza.observaciones}'
                                           ,'{finanza.registra_Usuario}'
                                           ,getDate()
                                           ,1)";
                    }

                    int activo = finanza.activo ? 1 : 0;

                    response.Code = connection.Execute(query);
                    response.Result = true;

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = false;
                response.Description = ex.Message;

            }
            return response;
        }

        #endregion

        #region Sintecos Financieros
        /// <summary>
        /// Obtengo la sintesis financiera del socio , es solo consulta 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneSintesisFinanzas> SintecisFinanciera_Obtener(int CodCliente, string? cedula)
        {
            var sintesis = new ErrorDto<AfiBeneSintesisFinanzas>();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string where = "";
                    if (cedula != null)
                    {
                        where = @$" CEDULA = '{cedula.Trim()}' AND ";
                    }
                    else
                    {
                        return sintesis;
                    }

                    var query = @$"SELECT
	                                     ISNULL( (SELECT SUM(MONTO * 0.9016) AS MONTO
			                                FROM  AFI_BENE_SOCIO_FINANZAS  
			                                WHERE  TIPO = 'I' AND ACTIVO = 1 AND CONCEPTO LIKE '%Salario%'
			                                AND CEDULA = '{cedula}' ),0) + ISNULL( (SELECT SUM(MONTO) AS MONTO
			                                FROM  AFI_BENE_SOCIO_FINANZAS  
			                                WHERE  TIPO = 'I' AND ACTIVO = 1 AND CONCEPTO NOT LIKE '%Salario%'
			                                AND CEDULA = '{cedula}' ) ,0) AS INGRESOS ,
                                           (SELECT SUM(ISNULL(CUOTA,0)) FROM AFI_BENE_SOCIO_FINANZAS
                                           WHERE CEDULA ='{cedula}'  AND TIPO = 'E' AND ACTIVO = 1 ) AS ENDEUDAMIENTO , 
                                           (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS
                                           WHERE CEDULA = '{cedula}'  AND TIPO = 'G' AND ACTIVO = 1) AS GASTOS ,
                                           (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS
                                           WHERE CEDULA = '{cedula}'  AND TIPO = 'GE' AND ACTIVO = 1) AS GASTO_ESPECIAL ,
                                          (SELECT COUNT(CEDULA_PARIENTE) + 1 FROM AFI_BENE_SOCIO_FAMILIA 
		                                   WHERE CEDULA = '{cedula}'  AND ACTIVO = 1) AS MIEMBROS ,
                                          (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS
                                           WHERE CEDULA = '{cedula}'  AND TIPO = 'M' AND ACTIVO = 1) AS MANUTENCION";
                    sintesis.Result = connection.Query<AfiBeneSintesisFinanzas>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                sintesis.Code = -1;
                sintesis.Description = ex.Message;
                sintesis.Result = null;
            }
            return sintesis;
        }

        #endregion

        #region Comportamiento financiero � Aseccss
        /// <summary>
        /// Obtengo el comportamiento financiero del socio, es solo consulta
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneCompFinanciero> ComportamientoFinanciero_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var lista = new ErrorDto<AfiBeneCompFinanciero>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spBene_Situacion_Financiera '{cedula}' ";
                    lista.Result = db.Query<AfiBeneCompFinanciero>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                lista.Code = -1;
                lista.Description = ex.Message;
                lista.Result = null;
            }
            return lista;
        }

        #endregion

        #region Justificaciones
        /// <summary>
        /// Obtengo la lista de motivos de justificaciones para el formulario de apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="categoria"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneMotivoLista_Obtener(int CodCliente, string? categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                if (categoria == null)
                {
                    return response;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" SELECT COD_MOTIVO AS ITEM, DESCRIPCION FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO IN (
                                            SELECT COD_MOTIVO FROM  AFI_BENE_GRUPO_MOTIVOS WHERE COD_GRUPO IN (
                                              SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = '{categoria}'
											                              )
                                            )";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = null;
                response.Description = ex.Message;
            }

            return response;
        }
        /// <summary>
        /// Obtengo la lista de justificaciones para el formulario de apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneApreJustificacion>> BeneJustificaciones_Obtener(int CodCliente, string cedula, int expediente)
        {
            var response = new ErrorDto<List<AfiBeneApreJustificacion>>();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"SELECT [ID_JUSTIFICACION]
                                      ,[COD_BENEFICIO]
                                      ,[CONSEC]
                                      ,[CEDULA]
                                      ,[JUST_LIST_ID]
                                      ,[JUSTIFICACION]
                                      ,[ADVERTENCIA]
                                      ,[ESTADO]
                                      ,[TIPO_BENEFICIO]
                                      ,[REGISTRO_FECHA]
                                      ,[REGISTRO_USUARIO]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO]
                                  FROM AFI_BENE_REGISTRO_JUSTIFICACIONES WHERE CEDULA = '{cedula.Trim()}' AND CONSEC = {expediente} ";
                    response.Result = connection.Query<AfiBeneApreJustificacion>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = null;
                response.Description = ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Guardo la justificacion del socio de benficio apremientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="justificacion"></param>
        /// <returns></returns>
        public ErrorDto BeneJustificacion_Guardar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {

            ErrorDto info = new ErrorDto();
            try
            {
                //valido si el miembro ya existe
                if (justificacion.id_justificacion != 0)
                {
                    info = BeneJustificacion_Actualizar(CodCliente, justificacion);
                }
                else
                {
                    info = BeneJustificacion_Insertar(CodCliente, justificacion);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Inserto un registro de justificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="justificacion"></param>
        /// <returns></returns>
        private ErrorDto BeneJustificacion_Insertar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //valido si el miembro ya existe
                    var query = $@"INSERT INTO [dbo].[AFI_BENE_REGISTRO_JUSTIFICACIONES]
                                           ([COD_BENEFICIO]
                                           ,[CONSEC]
                                           ,[CEDULA]
                                           ,[JUST_LIST_ID]
                                           ,[JUSTIFICACION]
                                           ,[ADVERTENCIA]
                                           ,[ESTADO]
                                           ,[TIPO_BENEFICIO]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO]
                                           )
                                     VALUES
                                           ('{justificacion.cod_beneficio}'
                                           ,{justificacion.consec}
                                           ,'{justificacion.cedula}'
                                           ,'{justificacion.just_list_id.item}'      
                                           ,'{justificacion.just_list_id.descripcion}'
                                           ,'{justificacion.advertencia}'
                                           ,'{justificacion.estado}'
                                           ,'{justificacion.tipo_beneficio}'
                                           , getDate()
                                           ,'{justificacion.registro_usuario}'
                                           )";

                    info.Code = connection.Execute(query);

                    query = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_JUSTIFICACIONES') as 'id'";
                    justificacion.id_justificacion = connection.Query<int>(query).FirstOrDefault();

                    info.Description = justificacion.id_justificacion.ToString();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Actualizo un registro de justificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="justificacion"></param>
        /// <returns></returns>
        private ErrorDto BeneJustificacion_Actualizar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el miembro ya existe
                    var query = $@"UPDATE AFI_BENE_REGISTRO_JUSTIFICACIONES
                                       SET 
                                           [JUST_LIST_ID] = '{justificacion.just_list_id.item}'
                                          ,[JUSTIFICACION] = '{justificacion.justificacion}'
                                          ,[ADVERTENCIA] = '{justificacion.advertencia}'
                                          ,[ESTADO] = '{justificacion.estado}'
                                          ,[MODIFICA_FECHA] = getDate()
                                          ,[MODIFICA_USUARIO] = '{justificacion.modifica_usuario}'
                                     WHERE ID_JUSTIFICACION = {justificacion.id_justificacion} ";

                    info.Code = connection.Execute(query);
                    info.Description = justificacion.id_justificacion.ToString();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
        /// <summary>
        /// Elimino un registro de justificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_justificacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto BeneJustificacion_Eliminar(int CodCliente, int id_justificacion, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el miembro ya existe
                    var query = $@"DELETE AFI_BENE_REGISTRO_JUSTIFICACIONES
                                         WHERE ID_JUSTIFICACION = {id_justificacion} ";

                    info.Code = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        /// <summary>
        /// Metodo para obtener el costo de la manutencion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<float> CostoManutencion_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<float>();
            try
            {
                string codManutencion = _config.GetSection("AFI_Beneficios").GetSection("CodManutencion").Value.ToString();
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = '{codManutencion}' ";
                    response.Result = connection.Query<float>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener el costo de la deduccion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<float> CostoDeduccion_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<float>();
            try
            {
                string codDeduccion = _config.GetSection("AFI_Beneficios").GetSection("CodDeduccion").Value.ToString();
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = '{codDeduccion}' ";
                    response.Result = connection.Query<float>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }
    }
}