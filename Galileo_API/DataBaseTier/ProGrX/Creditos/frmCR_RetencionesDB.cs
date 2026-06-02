using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRetencionesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrRetencionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la información de retenciones de crédito por id_solicitud.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="idSolicitud">Id de la solicitud</param>
        /// <returns>ErrorDto con la lista de retenciones</returns>
        public List<RetencionCreditoData> AF_CR_Retenciones_Obtener(int codEmpresa, int idSolicitud)
        {
            string query = @"
                select 
                    R.id_solicitud,
                    R.codigo,
                    C.descripcion,
                    R.cedula,
                    S.nombre,
                    R.cuota,
                    R.estado,
                    R.observacion,
                    R.fechaforp,
                    R.plazo,
                    R.amortiza,
                    R.cuotas_planilla,
                    R.cuotas_directas,
                    R.documento_referido,
                    R.prideduc,
                    R.userRec,
                    R.cod_destino,
                    R.garantia,
                    RTRIM(isnull(Gt.DESCRIPCION,'')) as GarantiaDesc,
                    RTRIM(isnull(Cd.DESCRIPCION,'')) as DestinoDesc,
                    R.Base_Calculo,
                    R.Cod_Divisa
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                inner join Socios S on R.cedula = S.cedula
                left join CRD_GARANTIA_TIPOS Gt on R.GARANTIA = Gt.GARANTIA
                left join CATALOGO_DESTINOS Cd on R.COD_DESTINO = Cd.COD_DESTINO
                where R.estadosol = 'F'
                  and (C.retencion = 'S' or C.poliza = 'S')
                  and R.id_solicitud = @IdSolicitud";
            var lista = DbHelper.ExecuteListQuery<RetencionCreditoData>(_portalDb, codEmpresa, query, new { IdSolicitud = idSolicitud });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene la lista de socios ordenados por nombre.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <returns>ErrorDto con la lista de socios</returns>
        public List<SocioData> AF_CR_Retenciones_ObtenerSocios(int codEmpresa)
        {
            string query = @"SELECT cedula, cedular, nombre FROM SOCIOS ORDER BY nombre";
            var lista = DbHelper.ExecuteListQuery<SocioData>(_portalDb, codEmpresa, query);
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene la lista de códigos y descripciones de catálogo con retención y no asociados a planes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <returns>ErrorDto con la lista de catálogo de retención</returns>
        public List<CatalogoRetencionData> AF_CR_Retenciones_ObtenerCatalogoRetencion(int codEmpresa)
        {
            string query = @"
                SELECT Codigo, Descripcion
                FROM catalogo
                WHERE Retencion = 'S'
                  AND Codigo NOT IN (
                      SELECT CODIGO_ASE
                      FROM FND_PLANES
                      GROUP BY CODIGO_ASE
                  )
                ORDER BY Codigo";
            var lista = DbHelper.ExecuteListQuery<CatalogoRetencionData>(_portalDb, codEmpresa, query);
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene el combo de deductoras por institución.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codInstitucion">Código de la institución</param>
        /// <returns>ErrorDto con la lista de deductoras</returns>
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDeductorasCombo(int codEmpresa, string codInstitucion)
        {
            string query = @"SELECT COD_DEDUCTORA AS item, DESCRIPCION AS descripcion FROM vAFI_Deductoras WHERE cod_institucion = @CodInstitucion";
            var lista = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { CodInstitucion = codInstitucion });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene la descripción y frecuencia de una institución.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codDeductora">Código de la deductora/institución</param>
        /// <returns>ErrorDto con la descripción y frecuencia</returns>
        public List<InstitucionFrecuenciaData> AF_CR_Retenciones_ObtenerInstitucionFrecuencia(int codEmpresa, string codDeductora)
        {
            string query = @"SELECT RTRIM(descripcion) AS Descripcion, ISNULL(Frecuencia,'M') AS Frecuencia_Id FROM instituciones WHERE cod_institucion = @CodDeductora";
            var lista = DbHelper.ExecuteListQuery<InstitucionFrecuenciaData>(_portalDb, codEmpresa, query, new { CodDeductora = codDeductora });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene los datos de deducción de un socio por cédula.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="cedula">Cédula del socio</param>
        /// <returns>ErrorDto con los datos de deducción</returns>
        public List<SocioDeduccionData> AF_CR_Retenciones_ObtenerSocioDeduccion(int codEmpresa, string cedula)
        {
            string query = @"
                SELECT 
                    S.nombre,
                    ISNULL(I.DEDUCCION_PLANILLA,0) AS Deduccion,
                    S.cod_institucion,
                    Ed.Cod_Institucion AS DeductoraCod,
                    Ed.Descripcion AS DeductoraDesc
                FROM Socios S
                INNER JOIN Instituciones I ON S.cod_institucion = I.cod_Institucion
                LEFT JOIN Instituciones Ed ON ISNULL(S.cod_deductora,S.cod_institucion) = Ed.cod_Institucion
                WHERE S.cedula = @Cedula";
            var lista = DbHelper.ExecuteListQuery<SocioDeduccionData>(_portalDb, codEmpresa, query, new { Cedula = cedula });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene la primera deducción calculada para una deductora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codDeductora">Código de la deductora/institución</param>
        /// <returns>Lista con la primera deducción calculada</returns>
        public List<PrimerDeduccionData> AF_CR_Retenciones_ObtenerPrimerDeduccion(int codEmpresa, string codDeductora)
        {
            string query = @"
                SELECT 
                    dbo.fxCrd_Primer_Deduccion(@CodDeductora) AS FechaCorte,
                    ISNULL(Frecuencia,'M') AS FrecuenciaId
                FROM instituciones
                WHERE cod_institucion = @CodDeductora";

            var lista = DbHelper.ExecuteListQuery<PrimerDeduccionRawData>(
                _portalDb,
                codEmpresa,
                query,
                new { CodDeductora = codDeductora }
            );

            var data = lista.Result?.FirstOrDefault();

            if (data == null)
                return [];

            int anio = data.FechaCorte.Year;
            int mes = data.FechaCorte.Month;
            int quincena = 0;

            if (data.FrecuenciaId == "Q")
            {
                quincena = data.FechaCorte.Day == 15 ? 1 : 2;
            }

            decimal primerDeduccion = decimal.Parse(
                $"{anio}{mes:00}.{quincena}",
                CultureInfo.InvariantCulture
            );

            return [
                new PrimerDeduccionData
                {
                    FechaCorte = data.FechaCorte,
                    FrecuenciaId = data.FrecuenciaId,
                    Anio = anio,
                    Mes = mes,
                    Quincena = quincena,
                    PrimerDeduccion = primerDeduccion
                }
];
        }

        /// <summary>
        /// Obtiene los destinos asociados a un código.
        /// </summary>
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDestinosPorCodigo(int codEmpresa, string codigo)
        {
            string query = @"
                select 
                    rtrim(D.cod_Destino) as item,
                    rtrim(D.descripcion) as descripcion
                from catalogo_destinos D
                inner join catalogo_destinosASG C on D.cod_destino = C.cod_destino
                where C.codigo = @Codigo
                order by D.prioridad asc";
            var lista = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { Codigo = codigo });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene las garantías asociadas a una línea.
        /// </summary>
        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerGarantiasPorLinea(int codEmpresa, string linea)
        {
            string query = @"
                select 
                    rtrim(T.Garantia) as item,
                    rtrim(T.descripcion) as descripcion
                from crd_catalogo_garantias C
                inner join crd_garantia_tipos T on C.garantia = T.garantia
                where C.codigo = @Linea";
            var lista = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { Linea = linea });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene el detalle de un catálogo por código.
        /// </summary>
        public List<CatalogoDetalleData> AF_CR_Retenciones_ObtenerCatalogoDetalle(int codEmpresa, string codigo)
        {
            string query = @"
                SELECT 
                    Cat.CODIGO,
                    Cat.DESCRIPCION,
                    Cat.MONEDA AS COD_DIVISA,
                    Cat.ID_COMITE,
                    ISNULL(Com.DESCRIPCION,'') AS COMITE_DESC,
                    Cat.BASE_CALCULO
                FROM CATALOGO Cat
                LEFT JOIN COMITES Com ON Cat.ID_COMITE = Com.ID_COMITE
                WHERE Cat.CODIGO = @Codigo";
            var lista = DbHelper.ExecuteListQuery<CatalogoDetalleData>(_portalDb, codEmpresa, query, new { Codigo = codigo });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Obtiene el siguiente o anterior id_solicitud de crédito con retención o póliza.
        /// </summary>
        public List<SiguienteSolicitudData> AF_CR_Retenciones_ObtenerSiguienteSolicitud(int codEmpresa, int idSolicitudActual, bool siguiente)
        {
            string operador = siguiente ? ">" : "<";
            string orden = siguiente ? "ASC" : "DESC";
            string query = $@"
                SELECT TOP 1 R.id_solicitud
                FROM reg_creditos R
                INNER JOIN Catalogo C ON R.codigo = C.codigo
                WHERE (C.retencion = 'S' OR C.poliza = 'S')
                  AND R.id_solicitud {operador} @IdSolicitudActual
                ORDER BY R.id_solicitud {orden}";
            var lista = DbHelper.ExecuteListQuery<SiguienteSolicitudData>(_portalDb, codEmpresa, query, new { IdSolicitudActual = idSolicitudActual });
            return lista.Result ?? [];
        }

        /// <summary>
        /// Inserta un nuevo crédito en reg_creditos.
        /// </summary>
        public ErrorDto AF_CR_Retenciones_InsertarCredito(int codEmpresa, InsertarCreditoRequest req)
        {
            string query = @"
                INSERT INTO reg_creditos (
                    codigo, id_comite, cedula, montosol, montoapr, monto_girado, saldo, amortiza, interesc, saldo_mes, cuota, int, interesv, plazo,
                    userrec, userres, userfor, usertesoreria, tesoreria, fechasol, fechares, fechaforp, fechaforf, fecha_calculo_int, garantia,
                    primer_cuota, tdocumento, ndocumento, pagare, firma_deudor, premio, observacion, estado, prideduc, fecult, estadosol,
                    documento_referido, cod_destino, cod_divisa, base_calculo
                )
                VALUES (
                    @Codigo, @IdComite, @Cedula, @MontoSol, @MontoApr, @MontoGirado, @Saldo, @Amortiza, @Interesc, @SaldoMes, @Cuota, @Int, @Interesv, @Plazo,
                    @UserRec, @UserRes, @UserFor, @UserTesoreria, @Tesoreria, @FechaSol, @FechaRes, @FechaForp, @FechaForf, @FechaCalculoInt, @Garantia,
                    @PrimerCuota, @TDocumento, @NDocumento, @Pagare, @FirmaDeudor, @Premio, @Observacion, @Estado, @PriDeduc, @FecUlt, @EstadoSol,
                    @DocumentoReferido, @CodDestino, @CodDivisa, @BaseCalculo
                )";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, req);
        }

        /// <summary>
        /// Valida existencia de catálogo, socio y obtiene ctaNintC antes de insertar.
        /// </summary>
        public ValidacionPreviaInsertarCreditoResponse AF_CR_Retenciones_ValidarAntesInsertar(int codEmpresa, string codigo, string cedula)
        {
            string queryCatalogo = "SELECT ISNULL(COUNT(*),0) FROM catalogo WHERE (Retencion = 'S' OR Poliza = 'S') AND codigo = @Codigo";
            string querySocio = "SELECT ISNULL(COUNT(*),0) FROM socios WHERE cedula = @Cedula";
            string queryCtaNintC = "SELECT ctaNintC FROM catalogo WHERE codigo = @Codigo";

            int existeCatalogo = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryCatalogo, 0, new { Codigo = codigo }).Result;
            int existeSocio = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, querySocio, 0, new { Cedula = cedula }).Result;
            string? ctaNintC = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, queryCtaNintC, null, new { Codigo = codigo }).Result;

            return new ValidacionPreviaInsertarCreditoResponse
            {
                ExisteCatalogo = existeCatalogo > 0,
                ExisteSocio = existeSocio > 0,
                CtaNintC = ctaNintC
            };
        }
    }
}
