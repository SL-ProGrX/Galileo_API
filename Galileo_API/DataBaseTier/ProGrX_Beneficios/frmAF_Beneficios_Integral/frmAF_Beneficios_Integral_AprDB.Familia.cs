using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralAprDB
    {
        /// <summary>
        /// Guarda un miembro del núcleo familiar: valida el socio y luego inserta o actualiza.
        /// </summary>
        public ErrorDto MiembroFamiliar_Guardar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            var valida = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, miembro.cedula.Trim());
            if (valida.Code == -1)
            {
                return new ErrorDto { Code = -1, Description = valida.Description };
            }

            try
            {
                return miembro.id_socio_familia != 0
                    ? MiembroFamiliar_Actualizar(CodCliente, miembro)
                    : MiembroFamiliar_Agregar(CodCliente, miembro);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Agrega un miembro del núcleo familiar (valida que no exista la misma cédula pariente).
        /// </summary>
        public ErrorDto MiembroFamiliar_Agregar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            const string sqlExiste = @"SELECT COUNT(*) FROM [dbo].[AFI_BENE_SOCIO_FAMILIA]
                                        WHERE CEDULA_PARIENTE = @cedulaPariente AND CEDULA = @cedula";

            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_SOCIO_FAMILIA]
                    ([CEDULA],[PARENTESCO],[APELLIDO_1],[APELLIDO_2],[NOMBRE],[NACIONALIDAD],[CEDULA_PARIENTE],[EDAD],
                     [ESTADO_CIVIL],[ACTIVIDAD_REALIZA],[OCUPACION],[DESEMPLEO],[CONDICION_ASEGURAMIENTO],[INGRESO_BRUTO],
                     [PENSION_TIPO],[DISCAPACIDAD_TIPO],[DISCAPACIDAD_DESC],[CENTRO_EDUCATIVO],[GRADO_ACADEMICO],
                     [ESTUDIANTE_BECADO],[EJERCE_CUIDO],[PAGO_X_CUIDO],[OBSERVACIONES],[REGISTRO_FECHA],[REGISTRO_USUARIO],[ACTIVO])
                VALUES
                    (@cedula,@parentesco,@apellido1,@apellido2,@nombre,@nacionalidad,@cedulaPariente,@edad,
                     @estadoCivil,@actividadRealiza,@ocupacion,@desempleo,@condicionAseguramiento,@ingresoBruto,
                     @pensionTipo,@discapacidadTipo,@discapacidadDesc,@centroEducativo,@gradoAcademico,
                     @estudianteBecado,@ejerceCuido,@pagoXCuido,@observaciones,GETDATE(),@registroUsuario,1)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_FAMILIA') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var count = connection.QueryFirstOrDefault<int>(sqlExiste,
                    new { cedulaPariente = miembro.cedula_pariente, cedula = miembro.cedula.Trim() });
                if (count > 0)
                {
                    return -1; // sentinel: ya existe
                }

                connection.Execute(sqlInsert, MapMiembroParametros(miembro));
                return connection.QueryFirstOrDefault<int>(sqlId);
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            if (result.Result == -1)
            {
                return new ErrorDto { Code = -1, Description = "Ya existe un miembro con la cedula ingresada" };
            }

            return new ErrorDto { Code = 0, Description = result.Result.ToString() };
        }

        /// <summary>
        /// Actualiza un miembro del núcleo familiar.
        /// </summary>
        private ErrorDto MiembroFamiliar_Actualizar(int CodCliente, BeneIntNucleoFamDto miembro)
        {
            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_SOCIO_FAMILIA]
                   SET [PARENTESCO] = @parentesco, [APELLIDO_1] = @apellido1, [APELLIDO_2] = @apellido2, [NOMBRE] = @nombre,
                       [NACIONALIDAD] = @nacionalidad, [CEDULA_PARIENTE] = @cedulaPariente, [EDAD] = @edad,
                       [ESTADO_CIVIL] = @estadoCivil, [ACTIVIDAD_REALIZA] = @actividadRealiza, [OCUPACION] = @ocupacion,
                       [DESEMPLEO] = @desempleo, [CONDICION_ASEGURAMIENTO] = @condicionAseguramiento, [INGRESO_BRUTO] = @ingresoBruto,
                       [PENSION_TIPO] = @pensionTipo, [DISCAPACIDAD_TIPO] = @discapacidadTipo, [DISCAPACIDAD_DESC] = @discapacidadDesc,
                       [CENTRO_EDUCATIVO] = @centroEducativo, [GRADO_ACADEMICO] = @gradoAcademico, [ESTUDIANTE_BECADO] = @estudianteBecado,
                       [EJERCE_CUIDO] = @ejerceCuido, [PAGO_X_CUIDO] = @pagoXCuido, [OBSERVACIONES] = @observaciones,
                       [MODIFICA_FECHA] = GETDATE(), [MODIFICA_USUARIO] = @modificaUsuario, [ACTIVO] = @activo
                 WHERE ID_SOCIO_FAMILIA = @idSocioFamilia";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, MapMiembroParametros(miembro)));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? miembro.id_socio_familia.ToString() : result.Description
            };
        }

        /// <summary>
        /// Obtiene los miembros activos del núcleo familiar de la cédula.
        /// </summary>
        public ErrorDto<List<BeneIntNucleoFamLista>> MiembrosFamiliar_Obtener(int CodCliente, string? cedula)
        {
            if (cedula == null)
            {
                return new ErrorDto<List<BeneIntNucleoFamLista>> { Code = 0, Description = "Ok", Result = new List<BeneIntNucleoFamLista>() };
            }

            const string sql = "SELECT * FROM AFI_BENE_SOCIO_FAMILIA WHERE CEDULA = @cedula AND ACTIVO = 1";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<BeneIntNucleoFamLista>(sql, new { cedula = cedula.Trim() }).ToList());

            return new ErrorDto<List<BeneIntNucleoFamLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<BeneIntNucleoFamLista>()
            };
        }

        /// <summary>
        /// Inactiva (elimina lógicamente) un miembro del núcleo familiar.
        /// </summary>
        public ErrorDto MiembroFamiliar_Eliminar(int CodCliente, long id, string usuario)
        {
            const string sql = @"UPDATE [dbo].[AFI_BENE_SOCIO_FAMILIA]
                                    SET [ACTIVO] = 0, [MODIFICA_FECHA] = GETDATE(), [MODIFICA_USUARIO] = @usuario
                                  WHERE ID_SOCIO_FAMILIA = @id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { usuario, id }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? "Miembro familiar eliminado correctamente" : result.Description
            };
        }

        /// <summary>
        /// Arma el objeto de parámetros compartido para insertar/actualizar un miembro del núcleo familiar.
        /// </summary>
        private static object MapMiembroParametros(BeneIntNucleoFamDto m) => new
        {
            idSocioFamilia = m.id_socio_familia,
            cedula = m.cedula,
            parentesco = ItemOrEmpty(m.parentesco),
            apellido1 = m.apellido_1,
            apellido2 = m.apellido_2,
            nombre = m.nombre,
            nacionalidad = ItemOrEmpty(m.nacionalidad),
            cedulaPariente = m.cedula_pariente,
            edad = m.edad,
            estadoCivil = ItemOrEmpty(m.estado_civil),
            actividadRealiza = ItemOrEmpty(m.actividad_realiza),
            ocupacion = m.ocupacion,
            desempleo = ItemOrEmpty(m.desempleo),
            condicionAseguramiento = ItemOrEmpty(m.condicion_aseguramiento),
            ingresoBruto = m.ingreso_bruto,
            pensionTipo = ItemOrEmpty(m.pension_tipo),
            discapacidadTipo = ItemOrEmpty(m.discapacidad_tipo),
            discapacidadDesc = m.discapacidad_desc,
            centroEducativo = m.centro_educativo,
            gradoAcademico = ItemOrEmpty(m.grado_academico),
            estudianteBecado = m.estudiante_becado ? 1 : 0,
            ejerceCuido = ItemOrEmpty(m.ejerce_cuido),
            pagoXCuido = m.pago_x_cuido,
            observaciones = m.observaciones,
            registroUsuario = m.registro_usuario,
            modificaUsuario = m.modifica_usuario,
            activo = m.activo ? 1 : 0
        };
    }
}
