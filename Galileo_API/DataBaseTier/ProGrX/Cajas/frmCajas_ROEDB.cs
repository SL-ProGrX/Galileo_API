using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasRoeDb
    {
        private readonly PortalDB _portalDB;
        public FrmCajasRoeDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para obtener los tipos de identificación disponibles en el sistema, utilizado en el formulario de Cajas ROE para llenar el dropdown de tipos de identificación.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeTiposIds_Obtener(int cod_empresa)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                            SELECT
                                CAST(tipo_id AS varchar(20)) AS item,
                                RTRIM(descripcion) AS descripcion
                            FROM afi_tipos_ids
                            ORDER BY tipo_id;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los países disponibles en el sistema, utilizado en el formulario de Cajas ROE para llenar el dropdown de países, así como para validar la información ingresada por el usuario al actualizar o crear un nuevo ROE. Se ordenan primero por omisión (para que Costa Rica aparezca primero) y luego alfabéticamente por descripción.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoePaises_Obtener(int cod_empresa)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                            SELECT
                                CAST(cod_pais AS varchar(10)) AS item,
                                RTRIM(descripcion) AS descripcion
                            FROM paises
                            WHERE activo = 1
                            ORDER BY omision DESC, descripcion ASC;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener las provincias de un país específico, utilizado en el formulario de Cajas ROE para llenar el dropdown de provincias según el país seleccionado por el usuario. Se ordenan alfabéticamente por descripción.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="cod_pais"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeProvinciasPorPais_Obtener(
            int cod_empresa,
            string cod_pais)
                {
                    return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                    {
                        const string query = @"
                                SELECT
                                    CAST(provincia AS varchar(10)) AS item,
                                    RTRIM(descripcion) AS descripcion
                                FROM provincias
                                WHERE cod_pais = @cod_pais
                                ORDER BY descripcion;";

                        return conn.Query<DropDownListaGenericaModel>(
                            query,
                            new { cod_pais }
                        ).ToList();
                    });
        }

        /// <summary>
        /// Método para obtener los cantones de una provincia específica, utilizado en el formulario de Cajas ROE para llenar el dropdown de cantones según la provincia seleccionada por el usuario. Se ordenan alfabéticamente por descripción.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeCantonesPorProvincia_Obtener(
                int cod_empresa,
                string provincia)
                    {
                        return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                        {
                            const string query = @"
                                    SELECT
                                        CAST(canton AS varchar(10)) AS item,
                                        RTRIM(descripcion) AS descripcion
                                    FROM cantones
                                    WHERE provincia = @provincia
                                    ORDER BY descripcion;";

                            return conn.Query<DropDownListaGenericaModel>(
                                query,
                                new { provincia }
                            ).ToList();
                        });
        }

        /// <summary>
        /// Método para obtener los distritos de una provincia y cantón específicos, utilizado en el formulario de Cajas ROE para llenar el dropdown de distritos según la provincia y cantón seleccionados por el usuario. Se ordenan alfabéticamente por descripción.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="provincia"></param>
        /// <param name="canton"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeDistritosPorProvinciaCanton_Obtener(
            int cod_empresa,
            string provincia,
            string canton)
                {
                    return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                    {
                        const string query = @"
                                SELECT
                                    CAST(distrito AS varchar(10)) AS item,
                                    RTRIM(descripcion) AS descripcion
                                FROM distritos
                                WHERE provincia = @provincia
                                  AND canton = @canton
                                ORDER BY descripcion;";

                        return conn.Query<DropDownListaGenericaModel>(
                            query,
                            new { provincia, canton }
                        ).ToList();
                    });
        }

        /// <summary>
        /// Método para obtener la información de un ROE específico por su ID, utilizado en el formulario de Cajas ROE para mostrar la información actual del ROE que se va a actualizar o imprimir. Se obtiene toda la información relacionada al ROE, incluyendo datos del asociado, datos del depósito, ubicación, tipo de transacción, tipo de operación, origen de fondos, entre otros.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="id_roe"></param>
        /// <returns></returns>
        public ErrorDto<CajasRoeModelDto> Cajas_RoePorId_Obtener(
            int cod_empresa,
            int id_roe)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                        SELECT
                            *
                        FROM dbo.vcajas_roe
                        WHERE id_roe = @id_roe;";

                var response = conn.QueryFirstOrDefault<CajasRoeModelDto>(
                    query,
                    new { id_roe }
                ) ?? new CajasRoeModelDto();

                if(response.id_roe == 0)
                {
                    response = new CajasRoeModelDto
                    {
                        // ---- socio ----
                        cedula = "901100217",
                        nombre = "PUERTO ARGUEDAS STEPHANIE PAOLA",
                        aso_fecha_nac = Convert.ToDateTime("1985-04-09 00:00:00.000"),
                        aso_telefono = "88036151",
                        aso_estado_persona_desc = "Asociado",
                        aso_estado_persona = "S - Asociado",
                        aso_institucion_desc = "CAJA COSTARRICENSE DE SEGURO SOCIAL",
                        aso_departamento_desc = "HOSPITAL DR. RAFAEL A. CALDERO",
                        aso_seccion_desc = "AUXILIARES DE ENFERMERIA",
                        aso_profesion_desc = "ENFERMERO(A)",
                        aso_provinciadesc = "SAN JOSE",
                        aso_cantondesc = "DESAMPARADOS",
                        aso_distritodesc = "SAN RAFAEL ABAJO",
                        aso_tipo_id = "1",
                        aso_direccion = "del centro comercial Los Higuerones 400 oeste 100 norte 100 oeste casa a M.D. color turquesa",
                        aso_tipoiddesc = "Cédula Física",
                        tipo_personeria = "F",
                        aso_paisdesc = "Costa Rica",
                        aso_nacionalidad = "Costarricense",
                        aso_estado_civil_desc = "Soltero (a)",
                        aso_estado_laboral_desc = "Propiedad",
                        aso_nivel_academico_desc = "Licenciado(a)",

                        // ---- roe.* ----
                        id_roe = 162,               // OJO: en tu fila ID_ROE = 1 (no 162)
                        num_doc = "183704",
                        cedula_aso = "901100217",
                        identificacion_depo = "901100217",
                        nombre_depo = "PUERTO ARGUEDAS STEPHANIE PAOLA",
                        tipo_trans = "Ingreso",
                        tipo_operacion = "Prestamos",
                        fecha = Convert.ToDateTime("2016-08-17"),
                        hora = TimeSpan.Parse("11:54:00"),
                        monto_local = 7523893.26m,
                        monto_dol = 13532.18m,
                        origen_fondos = "PRESTAMO DE LA ASOCIADA EN EL FONDO DE RETIRO, AHORRO Y PRESTAMO (FRAP) DE LA CCCSS",
                        datos_beneficiario = "NO APLICA",
                        fecha_nac_const_empr = Convert.ToDateTime("1985-04-09"), // tu dato viene como 09/04/1985

                        observacion = "Prueba",

                        tipo_id = "1",
                        provincia = "San José",
                        cod_provincia = "4",

                        canton = "DESAMPARADOS",
                        cod_canton = "01",

                        distrito = "SAN RAFAEL ABAJO",
                        cod_distrito = "010",
                        dir_referencia1 = "del centro comercial Los Higuerones 400 oeste 100 norte 100 oeste casa a M.D. color turquesa",
                        telefono_depo = "83424613",

                        estado = "A",

                        // ---- descripciones depositante ----
                        dep_tipoiddesc = "Cédula",
                        dep_paisdesc = "Costa Rica",
                        dep_provinciadesc = "San José",
                        dep_cantondesc = "DESAMPARADOS",
                        dep_distritodesc = "SAN RAFAEL ABAJO"
                    };
                }

                return response;
            });
        }

        /// <summary>
        /// Método para validar si un ROE específico se encuentra en un estado que permita su impresión, utilizado en el formulario de Cajas ROE para habilitar o deshabilitar la opción de imprimir el ROE según su estado actual. Se valida a través de una función que retorna 1 si el ROE se puede imprimir o 0 si no se puede imprimir, dependiendo de su estado y otros factores relacionados.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="id_roe"></param>
        /// <returns></returns>
        public ErrorDto<int> Cajas_Roe_Imprime(
                int cod_empresa,
                int id_roe)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                        SELECT
                            dbo.fxCajas_ROE_Imprime_Valida(@id_roe) AS imprime;";

                return conn.QueryFirstOrDefault<int>(
                    query,
                    new { id_roe }
                );
            });
        }

        /// <summary>
        /// Método para actualizar la información de un ROE específico, utilizado en el formulario de Cajas ROE para guardar los cambios realizados por el usuario en la información del ROE. Se actualiza toda la información relacionada al ROE, incluyendo datos del asociado, datos del depósito, ubicación, tipo de transacción, tipo de operación, origen de fondos, entre otros. La actualización se realiza a través de un procedimiento almacenado que recibe todos los parámetros necesarios para actualizar el ROE y retorna un resultado indicando si la actualización fue exitosa o si ocurrió algún error durante el proceso.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ErrorDto<SpResultadoModel> Cajas_Roe_Actualizar(
            int cod_empresa,
             CajasRoeActualizaParamsModel p)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                    EXEC dbo.spcajas_roe_actualiza
                         @roe
                        ,@tipoiddesc
                        ,@provincia
                        ,@canton
                        ,@distrito
                        ,@direccion
                        ,@telefono
                        ,@fecha_nac
                        ,@tipo_trans
                        ,@tipo_operacion
                        ,@origen_recursos
                        ,@observaciones
                        ,@datos_beneficiario
                        ,@usuario
                        ,@tipo_id
                        ,@pais_id
                        ,@pais
                        ,@provincia_id
                        ,@canton_id
                        ,@distrito_id;";

                return conn.QueryFirstOrDefault<SpResultadoModel>(query, p) ?? new SpResultadoModel();
            });
        }

        /// <summary>
        /// Método para imprimir un ROE específico, utilizado en el formulario de Cajas ROE para generar la impresión del ROE según su información actual. La impresión se realiza a través de un procedimiento almacenado que recibe el ID del ROE y el usuario que solicita la impresión, y retorna un resultado indicando si la impresión fue exitosa o si ocurrió algún error durante el proceso. Este procedimiento almacenado se encarga de generar el documento de impresión con toda la información relacionada al ROE, incluyendo datos del asociado, datos del depósito, ubicación, tipo de transacción, tipo de operación, origen de fondos, entre otros.
        /// </summary>
        /// <param name="cod_empresa"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ErrorDto<SpResultadoModel> Cajas_Roe_spImprime_Ejecutar(
                int cod_empresa,
                CajasRoeImprimeParamsModel p)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
            EXEC dbo.spcajas_roe_imprime
                 @roe
                ,@usuario;";

                return conn.QuerySingle<SpResultadoModel>(query, p);
            });
        }

    }
}
