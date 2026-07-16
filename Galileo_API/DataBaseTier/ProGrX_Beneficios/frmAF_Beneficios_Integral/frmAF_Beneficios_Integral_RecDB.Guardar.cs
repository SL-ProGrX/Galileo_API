using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_Integral_RecDB
    {
        /// <summary>
        /// Guarda el reconocimiento: valida duplicidad y luego inserta (id 0) o actualiza.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="reconocimiento">Datos del reconocimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneReconocimiento_Guardar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            const string sqlExiste = @"
                SELECT COUNT(*)
                FROM AFI_BENE_REGISTRO_RECONOCIMIENTOS R
                LEFT JOIN AFI_BENE_OTORGA O ON O.ID_BENEFICIO = R.ID_BENEFICIO
                WHERE R.CEDULA_ESTUDIANTE = @cedulaEstudiante
                  AND YEAR(R.RECONOCIMIENTO_FECHA) = YEAR(GETDATE())
                  AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')
                  AND O.ID_BENEFICIO != @idBeneficio";

            try
            {
                var existe = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                    connection.QueryFirstOrDefault<int>(sqlExiste, new
                    {
                        cedulaEstudiante = reconocimiento.cedula_estudiante,
                        idBeneficio = reconocimiento.id_beneficio
                    }));

                if (existe.Code != 0)
                {
                    return new ErrorDto { Code = -1, Description = "BeneReconocimiento_Guardar : " + existe.Description };
                }

                if (existe.Result > 0)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = $"El estudiante con la cédula {reconocimiento.cedula_estudiante} ya tiene un reconocimiento asignado."
                    };
                }

                return reconocimiento.id_reconocimiento != 0
                    ? BeneReconocimiento_Actualizar(CodCliente, reconocimiento)
                    : BeneReconocimiento_Ingresar(CodCliente, reconocimiento);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "BeneReconocimiento_Guardar : " + ex.Message };
            }
        }

        /// <summary>
        /// Inserta un nuevo reconocimiento y devuelve el id generado en Description.
        /// </summary>
        public ErrorDto BeneReconocimiento_Ingresar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_REGISTRO_RECONOCIMIENTOS]
                    ([COD_BENEFICIO],[CONSEC],[ID_BENEFICIO],[CEDULA_ESTUDIANTE],[FECHA_NACIMIENTO],[EDAD],
                     [GENERO],[PRIMER_APELLIDO],[SEGUNDO_APELLIDO],[NOMBRE],[TIPO_CENTRO],[CENTRO_EDUCATIVO],
                     [NIVEL_ACADEMICO],[GRADO],[OBSERVACIONES],[TIPO_RECONOCIMIENTO],[MATEMATICAS],[CIENCIAS],
                     [ESTUDIOS_SOCIALES],[ESPANOL],[IDIOMA],[RANGO],[RECONOCIMIENTO_ETAPA],[RECONOCIMIENTO_FECHA],
                     [RECONOCIMIENTO_NIVEL],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                VALUES
                    (@codBeneficio,@consec,@idBeneficio,@cedulaEstudiante,@fechaNacimiento,@edad,
                     @genero,@primerApellido,@segundoApellido,@nombre,@tipoCentro,@centroEducativo,
                     @nivelAcademico,@grado,@observaciones,@tipoReconocimiento,@matematicas,@ciencias,
                     @estudiosSociales,@espanol,@idioma,@rango,@reconocimientoEtapa,@reconocimientoFecha,
                     @reconocimientoNivel,GETDATE(),@registroUsuario)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_RECONOCIMIENTOS') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var filas = connection.Execute(sqlInsert, new
                {
                    codBeneficio = reconocimiento.cod_beneficio,
                    consec = reconocimiento.consec,
                    idBeneficio = reconocimiento.id_beneficio,
                    cedulaEstudiante = reconocimiento.cedula_estudiante,
                    fechaNacimiento = reconocimiento.fecha_nacimiento,
                    edad = reconocimiento.edad,
                    genero = ItemOrEmpty(reconocimiento.genero),
                    primerApellido = reconocimiento.primer_apellido,
                    segundoApellido = reconocimiento.segundo_apellido,
                    nombre = reconocimiento.nombre,
                    tipoCentro = ItemOrEmpty(reconocimiento.tipo_centro),
                    centroEducativo = reconocimiento.centro_educativo,
                    nivelAcademico = ItemOrEmpty(reconocimiento.nivel_academico),
                    grado = ItemOrEmpty(reconocimiento.grado),
                    observaciones = reconocimiento.observaciones,
                    tipoReconocimiento = ItemOrEmpty(reconocimiento.tipo_reconocimiento),
                    matematicas = reconocimiento.matematicas,
                    ciencias = reconocimiento.ciencias,
                    estudiosSociales = reconocimiento.estudios_sociales,
                    espanol = reconocimiento.espanol,
                    idioma = reconocimiento.idioma,
                    rango = ItemOrEmpty(reconocimiento.rango),
                    reconocimientoEtapa = ItemOrEmpty(reconocimiento.reconocimiento_etapa),
                    reconocimientoFecha = reconocimiento.reconocimiento_fecha,
                    reconocimientoNivel = ItemOrEmpty(reconocimiento.reconocimiento_nivel),
                    registroUsuario = reconocimiento.registro_usuario
                });

                reconocimiento.id_reconocimiento = connection.QueryFirstOrDefault<int>(sqlId);
                return filas;
            });

            return new ErrorDto
            {
                Code = result.Code == 0 ? result.Result : -1,
                Description = result.Code == 0 ? reconocimiento.id_reconocimiento.ToString() : result.Description
            };
        }

        /// <summary>
        /// Actualiza un reconocimiento existente.
        /// </summary>
        private ErrorDto BeneReconocimiento_Actualizar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_REGISTRO_RECONOCIMIENTOS]
                   SET [COD_BENEFICIO]        = @codBeneficio,
                       [CEDULA_ESTUDIANTE]    = @cedulaEstudiante,
                       [FECHA_NACIMIENTO]     = @fechaNacimiento,
                       [EDAD]                 = @edad,
                       [GENERO]               = @genero,
                       [PRIMER_APELLIDO]      = @primerApellido,
                       [SEGUNDO_APELLIDO]     = @segundoApellido,
                       [NOMBRE]               = @nombre,
                       [TIPO_CENTRO]          = @tipoCentro,
                       [CENTRO_EDUCATIVO]     = @centroEducativo,
                       [NIVEL_ACADEMICO]      = @nivelAcademico,
                       [GRADO]                = @grado,
                       [OBSERVACIONES]        = @observaciones,
                       [TIPO_RECONOCIMIENTO]  = @tipoReconocimiento,
                       [MATEMATICAS]          = @matematicas,
                       [CIENCIAS]             = @ciencias,
                       [ESTUDIOS_SOCIALES]    = @estudiosSociales,
                       [ESPANOL]              = @espanol,
                       [IDIOMA]               = @idioma,
                       [RANGO]                = @rango,
                       [RECONOCIMIENTO_ETAPA] = @reconocimientoEtapa,
                       [RECONOCIMIENTO_FECHA] = @reconocimientoFecha,
                       [RECONOCIMIENTO_NIVEL] = @reconocimientoNivel,
                       [MODIFICA_FECHA]       = GETDATE(),
                       [MODIFICA_USUARIO]     = @modificaUsuario
                 WHERE id_reconocimiento = @idReconocimiento";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, new
                {
                    codBeneficio = reconocimiento.cod_beneficio,
                    cedulaEstudiante = reconocimiento.cedula_estudiante,
                    fechaNacimiento = reconocimiento.fecha_nacimiento,
                    edad = reconocimiento.edad,
                    genero = ItemOrEmpty(reconocimiento.genero),
                    primerApellido = reconocimiento.primer_apellido,
                    segundoApellido = reconocimiento.segundo_apellido,
                    nombre = reconocimiento.nombre,
                    tipoCentro = ItemOrEmpty(reconocimiento.tipo_centro),
                    centroEducativo = reconocimiento.centro_educativo,
                    nivelAcademico = ItemOrEmpty(reconocimiento.nivel_academico),
                    grado = ItemOrEmpty(reconocimiento.grado),
                    observaciones = reconocimiento.observaciones,
                    tipoReconocimiento = ItemOrEmpty(reconocimiento.tipo_reconocimiento),
                    matematicas = reconocimiento.matematicas,
                    ciencias = reconocimiento.ciencias,
                    estudiosSociales = reconocimiento.estudios_sociales,
                    espanol = reconocimiento.espanol,
                    idioma = reconocimiento.idioma,
                    rango = ItemOrEmpty(reconocimiento.rango),
                    reconocimientoEtapa = ItemOrEmpty(reconocimiento.reconocimiento_etapa),
                    reconocimientoFecha = reconocimiento.reconocimiento_fecha,
                    reconocimientoNivel = ItemOrEmpty(reconocimiento.reconocimiento_nivel),
                    modificaUsuario = reconocimiento.modifica_usuario,
                    idReconocimiento = reconocimiento.id_reconocimiento
                }));

            return new ErrorDto
            {
                Code = result.Code == 0 ? result.Result : -1,
                Description = result.Code == 0 ? reconocimiento.id_reconocimiento.ToString() : result.Description
            };
        }

        /// <summary>
        /// Rechaza el expediente del reconocimiento (marca AFI_BENE_OTORGA con estado 'R').
        /// </summary>
        public ErrorDto BeneReconocimiento_Rechazar(int CodCliente, int id_beneficio, string usuario)
        {
            const string sql = @"
                UPDATE AFI_BENE_OTORGA
                   SET ESTADO = 'R', MODIFICA_USUARIO = @usuario, MODIFICA_FECHA = GETDATE()
                 WHERE id_beneficio = @idBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { usuario, idBeneficio = id_beneficio }));

            return new ErrorDto
            {
                Code = result.Code == 0 ? result.Result : -1,
                Description = result.Code == 0
                    ? "Expediente rechazado correctamente"
                    : "BeneReconocimiento_Rechazar: " + result.Description
            };
        }
    }
}
