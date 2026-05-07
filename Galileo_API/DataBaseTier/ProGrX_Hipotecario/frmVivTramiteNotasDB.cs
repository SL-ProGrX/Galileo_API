using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivTramiteNotasDb
    {
        private readonly PortalDB _portalDb;

        public FrmVivTramiteNotasDb(IConfiguration config)
           : this(
                 new PortalDB(config))
        {
        }

        public FrmVivTramiteNotasDb(PortalDB portalDB)
        {
            _portalDb = portalDB;
        }

        /// <summary>
        /// Obtiene la información del crédito y de la garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroOperacion"></param>
        /// <param name="idGarantia"></param>
        /// <returns></returns>
        public ErrorDto<VivTramiteNotaOperacionData?> VivTramiteNotas_ObtenerInformacionOperacion(
            int codEmpresa, string numeroOperacion, int idGarantia)
        {
            const string query = @"
            SELECT TOP 1
                RTRIM(ISNULL(RCR.ID_SOLICITUD, '')) AS numero_operacion,
                RTRIM(ISNULL(S.CEDULA, '')) AS cedula,
                RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
                RTRIM(ISNULL(ISNULL(CRDPreanalisis.COD_PREANALISIS, CRDPreanalisis.COD_PREANALISIS_REF), '')) AS expediente,
                RTRIM(ISNULL(Vgarantia.NumeroFinca, '')) AS numero_finca,
                RTRIM(ISNULL(Vgarantia.NumPlanoCatastro, '')) AS num_plano_catastro,
                RTRIM(ISNULL(vZona.Descripcion, '')) AS desc_zona,
                ISNULL(Vgarantia.AreaFinca, 0) AS area_finca,
                RTRIM(ISNULL(P.DESCRIPCION, '')) AS provincia,
                RTRIM(ISNULL(C.DESCRIPCION, '')) AS canton
            FROM ViviendaContactos AS vContactos
            INNER JOIN ViviendaGarantiaTramite AS vgTramite
                ON vContactos.IdContacto = vgTramite.IdContacto
            RIGHT OUTER JOIN PROVINCIAS AS P
            INNER JOIN CANTONES AS C
                ON P.PROVINCIA = C.PROVINCIA
            INNER JOIN REG_CREDITOS AS RCR
            INNER JOIN SOCIOS AS S
                ON RCR.CEDULA = S.CEDULA
            INNER JOIN ViviendaZonas AS vZona
            INNER JOIN ViviendaGarantia AS Vgarantia
                ON vZona.IdZona = Vgarantia.IdZona
                ON RCR.ID_SOLICITUD = Vgarantia.NumeroOperacion
                ON C.CANTON = Vgarantia.UbicacionCanton
                AND C.PROVINCIA = Vgarantia.UbicacionProvincia
                ON vgTramite.IdGarantia = Vgarantia.IdGarantia
            LEFT OUTER JOIN DISTRITOS AS D
                ON Vgarantia.UbicacionProvincia = D.PROVINCIA
                AND Vgarantia.UbicacionCanton = D.CANTON
                AND Vgarantia.UbicacionDistrito = D.DISTRITO
            LEFT OUTER JOIN CRD_PREA_PREANALISIS AS CRDPreanalisis
                ON RCR.ID_SOLICITUD = CRDPreanalisis.ID_SOLICITUD
            WHERE vgTramite.IdGarantia = @IdGarantia
              AND RTRIM(ISNULL(RCR.ID_SOLICITUD, '')) = @NumeroOperacion;";

            return DbHelper.ExecuteSingleQuery<VivTramiteNotaOperacionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    IdGarantia = idGarantia,
                    NumeroOperacion = NormalizarTexto(numeroOperacion)
                });
        }

        /// <summary>
        /// Obtiene el historial de notas de la garantía por tipo de profesional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idGarantia"></param>
        /// <param name="profesional"></param>
        /// <returns></returns>
        public ErrorDto<List<VivTramiteNotaData>> VivTramiteNotas_ObtenerLista(
            int codEmpresa, int idGarantia, string profesional)
        {
            const string query = @"
            SELECT
                ISNULL(VGT.IdNota, 0) AS id_nota,
                RTRIM(ISNULL(VGT.Nota, '')) AS nota,
                CASE RTRIM(ISNULL(VGT.Estado, ''))
                    WHEN 'R' THEN 'Garantía Registrada'
                    WHEN 'X' THEN 'Proceso de avaluo'
                    WHEN 'A' THEN 'Avaluo Registrado'
                    WHEN 'Y' THEN 'Proceso de registro'
                    WHEN 'S' THEN 'Solicitada'
                    ELSE ''
                END AS desc_estado,
                RTRIM(ISNULL(VGT.Usuario, '')) AS usuario,
                CONVERT(nvarchar(30), VGT.Fecha, 103) AS fecha_registro,
                ISNULL(VGT.IdGarantia, 0) AS id_garantia,
                ISNULL(VGT.IdContacto, 0) AS id_contacto,
                RTRIM(ISNULL(VGT.Tipo, '')) AS tipo,
                RTRIM(ISNULL(VGT.Estado, '')) AS estado,
                RTRIM(ISNULL(vcontactos.Nombre, '')) AS nombre,
                RTRIM(ISNULL(vGarantia.NumeroOperacion, '')) AS numero_operacion,
                RTRIM(ISNULL(vGarantia.NumeroFinca, '')) AS numero_finca
            FROM ViviendaGarantiaTramiteNotas AS VGT
            INNER JOIN ViviendaContactos AS vcontactos
                ON VGT.IdContacto = vcontactos.IdContacto
            INNER JOIN ViviendaGarantia AS vGarantia
                ON VGT.IdGarantia = vGarantia.IdGarantia
            WHERE VGT.IdGarantia = @IdGarantia
              AND VGT.Tipo = @Profesional
            ORDER BY VGT.Fecha DESC;";

            return DbHelper.ExecuteListQuery<VivTramiteNotaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdGarantia = idGarantia,
                    Profesional = NormalizarTexto(profesional)
                });
        }

        /// <summary>
        /// Inserta o modifica una nota de trámite.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivTramiteNotas_Guardar(
            int codEmpresa,  string usuario, VivTramiteNotaGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.nota))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe ingresar una nota."
                };
            }

            const string sql = @"
            EXEC spCRDVivGarantiaTramiteNotas_A
                @IdNota,
                @IdGarantia,
                @IdContacto,
                @Tipo,
                @Nota,
                @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdNota = request.id_nota,
                    IdGarantia = request.id_garantia,
                    IdContacto = request.id_contacto,
                    Tipo = NormalizarTexto(request.profesional),
                    Nota = NormalizarTexto(request.nota),
                    Usuario = usuario
                });
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }
    }
}