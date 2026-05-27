using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public partial class FrmVivMantenimientoDb
    {
        private const string ConsultaOperacionesGarantiaSql = @"
                SELECT
                    [key] = CONCAT('(vv)', G.NumeroOperacion, '(Op)', G.IdGarantia, '(Ig)', RTRIM(S.Cedula), '(Cd)'),
                    columna_1 = CAST(G.NumeroOperacion AS varchar(30)),
                    columna_2 = FORMAT(R.MONTOAPR, 'N2'),
                    columna_3 = RTRIM(S.Cedula),
                    columna_4 = RTRIM(S.Nombre),
                    columna_5 = CAST(G.NumeroFinca AS varchar(30)),
                    columna_6 = CASE G.Estado WHEN 'R' THEN 'Garantia Registrada' WHEN 'X' THEN 'Proceso de avaluo' WHEN 'A' THEN 'Avaluo Registrado' WHEN 'Y' THEN 'Proceso de registro' WHEN 'S' THEN 'Solicitada' ELSE '' END,
                    columna_7 = RTRIM(G.NumPlanoCatastro),
                    columna_8 = RTRIM(Z.Descripcion),
                    columna_9 = RTRIM(P.Descripcion),
                    columna_10 = RTRIM(C.Descripcion),
                    columna_11 = ISNULL(RTRIM(D.Descripcion), '')
                FROM Socios S
                INNER JOIN REG_CREDITOS R ON S.Cedula = R.Cedula
                INNER JOIN ViviendaGarantia G ON G.NumeroOperacion = R.ID_SOLICITUD
                INNER JOIN ViviendaZonas Z ON Z.IdZona = G.IdZona
                INNER JOIN Provincias P ON G.UbicacionProvincia = P.Provincia
                INNER JOIN Cantones C ON G.UbicacionProvincia = C.Provincia AND G.UbicacionCanton = C.Canton
                LEFT JOIN Distritos D ON G.UbicacionProvincia = D.Provincia AND G.UbicacionCanton = D.Canton AND G.UbicacionDistrito = D.Distrito ";

        /// <summary>
        /// Obtiene los nodos hijos del arbol de mantenimiento de garantias hipotecarias.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tag"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosHijos_Obtener(int codEmpresa, string tag, string key)
        {
            return NormalizarTexto(tag) switch
            {
                "ModuloDeVivienda" => FrmVivMantenimientoDb.VivMantenimiento_ArbolInicial_Obtener(),
                "NodoZonas" => VivMantenimiento_NodosZonas_Obtener(codEmpresa),
                "NodoZonasHijo" => DbHelper.CreateOkResponse(VivMantenimiento_NodosZonaDetalle_Obtener(key)),
                "NodoProfesionales" => DbHelper.CreateOkResponse(NodosProfesionales()),
                NodoEmpresas => VivMantenimiento_NodosEmpresas_Obtener(codEmpresa),
                "NodoEmpresaHijo" => DbHelper.CreateOkResponse(NodosEmpresaProfesionales(key)),
                "NodoAsigIngEmpresa" => VivMantenimiento_NodosProfesionalesEmpresa_Obtener(codEmpresa, key, "I"),
                "NodoAsigAbogEmpresa" => VivMantenimiento_NodosProfesionalesEmpresa_Obtener(codEmpresa, key, "A"),
                NodoPersonasFisicas => DbHelper.CreateOkResponse(NodosPersonasFisicas()),
                NodoAsigIngPf => VivMantenimiento_NodosPersonasFisicas_Obtener(codEmpresa, "I"),
                NodoAsigAbogPf => VivMantenimiento_NodosPersonasFisicas_Obtener(codEmpresa, "A"),
                "NodoIngZanasHijo" => VivMantenimiento_NodosProfesionalesZona_Obtener(codEmpresa, key, "I"),
                "NodoAbogZanasHijo" => VivMantenimiento_NodosProfesionalesZona_Obtener(codEmpresa, key, "A"),
                "NodoIngenierosZonaHijo" or "NodoAbogadosZonaHijo" or "NodoIngenierosEmpresaHijo" or "NodoAbogadosEmpresaHijo" or "NodoIngenierosPFHijo" or "NodoAbogadosPFHijo" => DbHelper.CreateOkResponse(NodosOperacionesProfesional(tag, key)),
                _ => DbHelper.CreateOkResponse(new List<VivMantenimientoNodoData>())
            };
        }

        /// <summary>
        /// Obtiene el detalle de lista asociado al nodo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tag"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Lista_Obtener(int codEmpresa, string tag, string key)
        {
            return NormalizarTexto(tag) switch
            {
                "NodoParametrosGenerales" => VivMantenimiento_Parametros_Obtener(codEmpresa),
                "NodoZonas" => VivMantenimiento_Zonas_Obtener(codEmpresa, null),
                "NodoZonasHijo" => VivMantenimiento_UbicacionZona_Obtener(codEmpresa, ObtenerLongKey(key, "(id)")),
                "NodoTiposDesembolsos" => VivMantenimiento_TiposDesembolsos_Obtener(codEmpresa),
                "NodoTiemposSeguimiento" => VivMantenimiento_TiemposSeguimiento_Obtener(codEmpresa),
                "NodoOperacionesTramite" => VivMantenimiento_OperacionesEnTramite_Obtener(codEmpresa),
                "NodoControlDesembolso" => VivMantenimiento_ControlDesembolso_Obtener(codEmpresa, false),
                "NodoOperacionesCanceladas" => VivMantenimiento_ControlDesembolso_Obtener(codEmpresa, true),
                "NodoTramiteGarantia" => VivMantenimiento_TramiteOperaciones_Obtener(codEmpresa),
                "NodoEmpresaHijo" => VivMantenimiento_ContactosEmpresa_Obtener(codEmpresa, ObtenerLongKey(key, "(Em)")),
                NodoEmpresas => VivMantenimiento_Empresas_Obtener(codEmpresa),
                "NodoAsigIngEmpresa" => VivMantenimiento_ContactosTipoEmpresa_Obtener(codEmpresa, ObtenerLongKey(key, "(Em)"), "I"),
                "NodoAsigAbogEmpresa" => VivMantenimiento_ContactosTipoEmpresa_Obtener(codEmpresa, ObtenerLongKey(key, "(Em)"), "A"),
                NodoPersonasFisicas => VivMantenimiento_PersonasFisicas_Obtener(codEmpresa, ""),
                NodoAsigIngPf => VivMantenimiento_PersonasFisicas_Obtener(codEmpresa, "I"),
                NodoAsigAbogPf => VivMantenimiento_PersonasFisicas_Obtener(codEmpresa, "A"),
                "NodoIngZanasHijo" => VivMantenimiento_ProfesionalesZona_Obtener(codEmpresa, key, "I"),
                "NodoAbogZanasHijo" => VivMantenimiento_ProfesionalesZona_Obtener(codEmpresa, key, "A"),
                "NodoIngenierosEmpresaHijo" or "NodoAbogadosEmpresaHijo" or "NodoIngenierosPFHijo" or "NodoAbogadosPFHijo" => VivMantenimiento_Zonas_Obtener(codEmpresa, ObtenerLongKey(key, "(ic)")),
                "NodoOperaAbogZonaTram" or "NodoOperaAbogZonaEje" or "NodoOperaIngZonaTram" or "NodoOperaIngZonaEje" or "NodoOperaAbogEmpresaTram" or "NodoOperaAbogEmpresaEje" or "NodoOperaIngEmpresaTram" or "NodoOperaIngEmpresaEje" or "NodoOperaAbogPFTram" or "NodoOperaAbogPFEje" or "NodoOperaIngPFTram" or "NodoOperaIngPFEje" => VivMantenimiento_OperacionesProfesional_Obtener(codEmpresa, tag, key),
                _ => DbHelper.CreateOkResponse(new List<VivMantenimientoListaData>())
            };
        }

        private ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosZonas_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(Vv)', IdZona, '(id)'),
                    label = RTRIM(Descripcion),
                    tag = 'NodoZonasHijo',
                    icon = 'pi pi-map-marker',
                    formulario = 'frmVivZonas',
                    ruta = '/viv-zonas',
                    leaf = CAST(0 AS bit)
                FROM ViviendaZonas
                ORDER BY Descripcion";

            return DbHelper.ExecuteListQuery<VivMantenimientoNodoData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosEmpresas_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(Vv)', IdEmpresa, '(Em)', IdContacto, '(ic)', RTRIM(Identificacion), '(Ie)'),
                    label = RTRIM(Nombre),
                    tag = 'NodoEmpresaHijo',
                    icon = 'pi pi-building',
                    formulario = 'frmVivProfesionales',
                    ruta = '/viv-informacion-profesionales',
                    leaf = CAST(0 AS bit)
                FROM ViviendaContactos
                WHERE TipoContacto = 'E' AND IdEmpresa IS NOT NULL
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoNodoData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosProfesionalesEmpresa_Obtener(int codEmpresa, string key, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdEmpresa, '(Em)', IdContacto, '(ic)', RTRIM(Identificacion), '(Ie)'),
                    label = RTRIM(Nombre),
                    tag = CASE @TipoProfesional WHEN 'A' THEN 'NodoAbogadosEmpresaHijo' ELSE 'NodoIngenierosEmpresaHijo' END,
                    icon = CASE @TipoProfesional WHEN 'A' THEN 'pi pi-briefcase' ELSE 'pi pi-user' END,
                    formulario = 'frmVivProfesionales',
                    ruta = '/viv-informacion-profesionales',
                    leaf = CAST(0 AS bit)
                FROM ViviendaContactos
                WHERE IdEmpresa = @IdEmpresa
                    AND TipoProfesional = @TipoProfesional
                    AND TipoContacto = 'C'
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoNodoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdEmpresa = ObtenerLongKey(key, "(Em)"), TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosPersonasFisicas_Obtener(int codEmpresa, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdContacto, '(Ic)', ISNULL(IdEmpresa, -1), '(Ie)', RTRIM(Identificacion), '(Id)', RTRIM(TipoProfesional), '(Tp)'),
                    label = RTRIM(Nombre),
                    tag = CASE @TipoProfesional WHEN 'A' THEN 'NodoAbogadosPFHijo' ELSE 'NodoIngenierosPFHijo' END,
                    icon = CASE @TipoProfesional WHEN 'A' THEN 'pi pi-briefcase' ELSE 'pi pi-user' END,
                    formulario = 'frmVivProfesionales',
                    ruta = '/viv-informacion-profesionales',
                    leaf = CAST(0 AS bit)
                FROM ViviendaContactos
                WHERE IdEmpresa IS NULL
                    AND TipoContacto = 'F'
                    AND TipoProfesional = @TipoProfesional
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoNodoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosProfesionalesZona_Obtener(int codEmpresa, string key, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', X.IdZona, '(Iz)', RTRIM(C.Identificacion), '(Ie)', C.IdContacto, '(Ic)', ISNULL(C.IdEmpresa, -1), '(Em)', RTRIM(C.TipoProfesional), '(Tp)'),
                    label = RTRIM(C.Nombre),
                    tag = CASE @TipoProfesional WHEN 'A' THEN 'NodoAbogadosZonaHijo' ELSE 'NodoIngenierosZonaHijo' END,
                    icon = CASE @TipoProfesional WHEN 'A' THEN 'pi pi-briefcase' ELSE 'pi pi-user' END,
                    formulario = 'frmVivProfesionales',
                    ruta = '/viv-informacion-profesionales',
                    leaf = CAST(0 AS bit)
                FROM ViviendaContactosXZona X
                INNER JOIN ViviendaContactos C ON X.IdContacto = C.IdContacto
                WHERE X.IdZona = @IdZona
                    AND C.TipoProfesional = @TipoProfesional
                ORDER BY C.Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoNodoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdZona = ObtenerLongKey(key, "(id)"), TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Parametros_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', RTRIM(CodigoParametro), '(id)'),
                    columna_1 = RTRIM(CodigoParametro),
                    columna_2 = RTRIM(Descripcion),
                    columna_3 = RTRIM(Valor)
                FROM ViviendaParametros
                ORDER BY CodigoParametro";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Zonas_Obtener(int codEmpresa, long? idContacto)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', Z.IdZona, '(id)'),
                    columna_1 = RTRIM(Z.Descripcion),
                    columna_2 = CASE Z.Activa WHEN 1 THEN 'Activa' WHEN 0 THEN 'Inactiva' ELSE '' END,
                    columna_3 = CAST(Z.IdZona AS varchar(20)),
                    columna_4 = CASE WHEN ISNULL(X.IdZona, -1) <> -1 THEN 'M' ELSE 'N' END
                FROM ViviendaZonas Z
                LEFT JOIN ViviendaContactosXZona X
                    ON Z.IdZona = X.IdZona
                    AND X.IdContacto = @IdContacto
                ORDER BY Z.Descripcion";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdContacto = idContacto ?? 0 });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_TiposDesembolsos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', RTRIM(Codigo), '(Cd)'),
                    columna_1 = RTRIM(Codigo),
                    columna_2 = RTRIM(Descripcion),
                    columna_3 = CASE estado WHEN 'A' THEN 'Activo' ELSE 'Inactivo' END
                FROM ViviendaTiposDesembolsos
                ORDER BY Descripcion";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_TiemposSeguimiento_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', RTRIM(Profesional), '(Pf)', RTRIM(Proceso), '(Pc)'),
                    columna_1 = CASE Profesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE Profesional END,
                    columna_2 = CASE Proceso
                        WHEN 'E' THEN 'Entrega de Garantia'
                        WHEN 'F' THEN 'Registro de Firmas'
                        WHEN 'I' THEN 'Inscripcion de garantia'
                        WHEN 'X' THEN 'Recepcion Informacion Avaluo'
                        WHEN 'R' THEN 'Registro Informacion Avaluo'
                        ELSE Proceso END,
                    columna_3 = CAST(TiempoMaximo AS varchar(20)),
                    columna_4 = CAST(TiempoAlerta AS varchar(20))
                FROM ViviendaTiemposSeguimiento
                ORDER BY Profesional, orden";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_UbicacionZona_Obtener(int codEmpresa, long idZona)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(Vv)', A.IdZona, '(Iz)', C.Canton, '(Ct)', P.Provincia, '(Pr)'),
                    columna_1 = RTRIM(P.Descripcion),
                    columna_2 = RTRIM(C.Descripcion),
                    columna_3 = CASE Z.Activa WHEN 1 THEN 'Activa' WHEN 0 THEN 'Inactiva' ELSE '' END
                FROM ViviendaZonaAsigna A
                INNER JOIN ViviendaZonas Z ON A.IdZona = Z.IdZona
                INNER JOIN Cantones C ON A.Provincia = C.Provincia AND A.Canton = C.Canton
                INNER JOIN Provincias P ON A.Provincia = P.Provincia
                WHERE Z.IdZona = @IdZona
                ORDER BY P.Descripcion, C.Descripcion";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql, new { IdZona = idZona });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Empresas_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdContacto, '(Dc)', IdEmpresa, '(Ie)', RTRIM(Identificacion), '(id)'),
                    columna_1 = RTRIM(Identificacion),
                    columna_2 = RTRIM(Nombre),
                    columna_3 = CASE TipoProfesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE '' END
                FROM ViviendaContactos
                WHERE TipoContacto = 'E' AND IdEmpresa IS NOT NULL
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_ContactosEmpresa_Obtener(int codEmpresa, long idEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdContacto, '(Dc)', IdEmpresa, '(Ie)', RTRIM(Identificacion), '(id)'),
                    columna_1 = RTRIM(Identificacion),
                    columna_2 = RTRIM(Nombre),
                    columna_3 = CASE TipoProfesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE '' END
                FROM ViviendaContactos
                WHERE IdEmpresa = @IdEmpresa AND TipoContacto = 'C'
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql, new { IdEmpresa = idEmpresa });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_ContactosTipoEmpresa_Obtener(int codEmpresa, long idEmpresa, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdContacto, '(Dc)', IdEmpresa, '(Ie)', RTRIM(Identificacion), '(Id)', RTRIM(TipoProfesional), '(Tp)'),
                    columna_1 = RTRIM(Identificacion),
                    columna_2 = RTRIM(Nombre),
                    columna_3 = CASE TipoProfesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE '' END
                FROM ViviendaContactos
                WHERE IdEmpresa = @IdEmpresa
                    AND TipoProfesional = @TipoProfesional
                    AND TipoContacto = 'C'
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdEmpresa = idEmpresa, TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_PersonasFisicas_Obtener(int codEmpresa, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', IdContacto, '(Dc)', ISNULL(IdEmpresa, -1), '(Ie)', RTRIM(Identificacion), '(Id)', RTRIM(TipoProfesional), '(Tp)'),
                    columna_1 = RTRIM(Identificacion),
                    columna_2 = RTRIM(Nombre),
                    columna_3 = CASE TipoProfesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE '' END
                FROM ViviendaContactos
                WHERE IdEmpresa IS NULL
                    AND TipoContacto = 'F'
                    AND (@TipoProfesional = '' OR TipoProfesional = @TipoProfesional)
                ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_ProfesionalesZona_Obtener(int codEmpresa, string key, string tipoProfesional)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(VV)', X.IdZona, '(Iz)', RTRIM(C.Identificacion), '(Ie)', ISNULL(C.IdEmpresa, -1), '(Em)', RTRIM(C.TipoProfesional), '(Tp)'),
                    columna_1 = RTRIM(C.Identificacion),
                    columna_2 = RTRIM(C.Nombre),
                    columna_3 = CASE C.TipoProfesional WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE '' END,
                    columna_4 = ISNULL(RTRIM(E.Nombre), '')
                FROM ViviendaContactosXZona X
                INNER JOIN ViviendaContactos C ON X.IdContacto = C.IdContacto
                LEFT JOIN ViviendaContactos E ON C.IdEmpresa = E.IdContacto
                WHERE X.IdZona = @IdZona
                    AND C.TipoProfesional = @TipoProfesional
                ORDER BY C.Nombre";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdZona = ObtenerLongKey(key, "(id)"), TipoProfesional = tipoProfesional });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_OperacionesEnTramite_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', R.ID_SOLICITUD, '(Op)', RTRIM(S.Cedula), '(Cd)'),
                    columna_1 = CAST(R.ID_SOLICITUD AS varchar(30)),
                    columna_2 = FORMAT(R.MONTOAPR, 'N2'),
                    columna_3 = RTRIM(S.Cedula),
                    columna_4 = RTRIM(S.Nombre),
                    columna_5 = CASE R.ESTADOSOL WHEN 'P' THEN 'Pendiente' WHEN 'R' THEN 'Recibida' ELSE R.ESTADOSOL END
                FROM Socios S
                INNER JOIN REG_CREDITOS R ON S.Cedula = R.Cedula
                WHERE R.GARANTIA = 'H'
                    AND R.ESTADOSOL IN ('R', 'P')
                ORDER BY R.ID_SOLICITUD DESC";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_ControlDesembolso_Obtener(int codEmpresa, bool canceladas)
        {
            const string sql = @"
                SELECT
                    [key] = CONCAT('(vv)', R.ID_SOLICITUD, '(Op)', RTRIM(S.Cedula), '(Cd)'),
                    columna_1 = CAST(R.ID_SOLICITUD AS varchar(30)),
                    columna_2 = FORMAT(R.MONTOAPR, 'N2'),
                    columna_3 = RTRIM(S.Cedula),
                    columna_4 = RTRIM(S.Nombre),
                    columna_5 = CASE R.ESTADOSOL WHEN 'P' THEN 'Pendiente' WHEN 'R' THEN 'Recibida' WHEN 'F' THEN 'Formalizada' ELSE R.ESTADOSOL END
                FROM Socios S
                INNER JOIN REG_CREDITOS R ON S.Cedula = R.Cedula
                INNER JOIN ViviendaDesembolsosDisponible D ON R.ID_SOLICITUD = D.NumeroOperacion
                WHERE R.ESTADOSOL = 'F'
                    AND R.EMITIR NOT IN ('CK', 'TE')
                    AND R.GARANTIA = 'H'
                    AND ((@Canceladas = 1 AND D.Disponible = 0) OR (@Canceladas = 0 AND D.Disponible > 0))
                ORDER BY R.ID_SOLICITUD DESC";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql, new { Canceladas = canceladas });
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_TramiteOperaciones_Obtener(int codEmpresa)
        {
            string sql = ConsultaOperacionesGarantiaSql + @"
                WHERE R.GARANTIA = 'H' AND R.ESTADOSOL <> 'F'
                ORDER BY G.NumeroOperacion DESC";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_OperacionesProfesional_Obtener(int codEmpresa, string tag, string key)
        {
            const string sqlBase = @"
                SELECT
                    [key] = CONCAT('(vv)', G.NumeroOperacion, '(Op)', G.IdGarantia, '(Ig)', RTRIM(S.Cedula), '(Cd)'),
                    columna_1 = CAST(G.NumeroOperacion AS varchar(30)),
                    columna_2 = FORMAT(R.MONTOAPR, 'N2'),
                    columna_3 = RTRIM(S.Cedula),
                    columna_4 = RTRIM(S.Nombre),
                    columna_5 = CAST(G.NumeroFinca AS varchar(30)),
                    columna_6 = CASE G.Estado WHEN 'R' THEN 'Garantia Registrada' WHEN 'X' THEN 'Proceso de avaluo' WHEN 'A' THEN 'Avaluo Registrado' WHEN 'Y' THEN 'Proceso de registro' WHEN 'S' THEN 'Solicitada' ELSE '' END,
                    columna_7 = RTRIM(G.NumPlanoCatastro),
                    columna_8 = RTRIM(Z.Descripcion),
                    columna_9 = RTRIM(P.Descripcion),
                    columna_10 = RTRIM(C.Descripcion),
                    columna_11 = ISNULL(RTRIM(D.Descripcion), '')
                FROM Socios S
                INNER JOIN REG_CREDITOS R ON S.Cedula = R.Cedula
                INNER JOIN ViviendaGarantia G ON G.NumeroOperacion = R.ID_SOLICITUD
                INNER JOIN ViviendaGarantiaTramite T ON G.IdGarantia = T.IdGarantia
                INNER JOIN ViviendaContactos VC ON T.IdContacto = VC.IdContacto
                INNER JOIN ViviendaZonas Z ON Z.IdZona = G.IdZona
                INNER JOIN Provincias P ON G.UbicacionProvincia = P.Provincia
                INNER JOIN Cantones C ON G.UbicacionProvincia = C.Provincia AND G.UbicacionCanton = C.Canton
                LEFT JOIN Distritos D ON G.UbicacionProvincia = D.Provincia AND G.UbicacionCanton = D.Canton AND G.UbicacionDistrito = D.Distrito
                WHERE VC.IdContacto = @IdContacto
                    AND T.Tipo = @TipoProfesional";

            var sql = EsOperacionEjecutada(tag)
                ? sqlBase + " AND R.EstadoSol IN ('A', 'C') ORDER BY G.NumeroOperacion DESC"
                : sqlBase + " ORDER BY G.NumeroOperacion DESC";

            return DbHelper.ExecuteListQuery<VivMantenimientoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdContacto = ObtenerIdContactoProfesional(key), TipoProfesional = ObtenerTipoProfesional(tag) });
        }

        private static List<VivMantenimientoNodoData> NodosProfesionales()
        {
            return new List<VivMantenimientoNodoData>
            {
                CrearNodo(NodoEmpresas, "Empresas", NodoEmpresas, "pi pi-building", FormVivProfesionales, RutaVivProfesionales, leaf: false),
                CrearNodo(NodoPersonasFisicas, "Personas Fisicas", NodoPersonasFisicas, "pi pi-users", FormVivProfesionales, RutaVivProfesionales, leaf: false)
            };
        }

        private static List<VivMantenimientoNodoData> NodosPersonasFisicas()
        {
            return new List<VivMantenimientoNodoData>
            {
                CrearNodo(NodoAsigIngPf, "Ingenieros", NodoAsigIngPf, "pi pi-user", FormVivProfesionales, RutaVivProfesionales, leaf: false),
                CrearNodo(NodoAsigAbogPf, "Abogados", NodoAsigAbogPf, "pi pi-briefcase", FormVivProfesionales, RutaVivProfesionales, leaf: false)
            };
        }

        private static List<VivMantenimientoNodoData> NodosEmpresaProfesionales(string key)
        {
            return new List<VivMantenimientoNodoData>
            {
                CrearNodo($"{key}|I", "Ingenieros", "NodoAsigIngEmpresa", "pi pi-user", FormVivProfesionales, RutaVivProfesionales, leaf: false),
                CrearNodo($"{key}|A", "Abogados", "NodoAsigAbogEmpresa", "pi pi-briefcase", FormVivProfesionales, RutaVivProfesionales, leaf: false)
            };
        }

        private static List<VivMantenimientoNodoData> VivMantenimiento_NodosZonaDetalle_Obtener(string key)
        {
            return new List<VivMantenimientoNodoData>
            {
                CrearNodo($"{key}|I", "Ingenieros", "NodoIngZanasHijo", "pi pi-user", FormVivProfesionales, RutaVivProfesionales, leaf: false),
                CrearNodo($"{key}|A", "Abogados", "NodoAbogZanasHijo", "pi pi-briefcase", FormVivProfesionales, RutaVivProfesionales, leaf: false)
            };
        }

        private static List<VivMantenimientoNodoData> NodosOperacionesProfesional(string tag, string key)
        {
            var esAbogado = tag.Contains("Abog", StringComparison.OrdinalIgnoreCase);
            var origen = ObtenerOrigenOperacionesProfesional(tag);
            var prefijo = esAbogado ? "NodoOperaAbog" : "NodoOperaIng";

            return new List<VivMantenimientoNodoData>
            {
                CrearNodo($"{key}|T", "Operaciones Tramite", $"{prefijo}{origen}Tram", "pi pi-file-edit"),
                CrearNodo($"{key}|E", "Operaciones Ejecutadas", $"{prefijo}{origen}Eje", "pi pi-check-circle")
            };
        }

        private static string NormalizarTexto(string valor)
            => (valor ?? string.Empty).Trim();

        private static long ObtenerLongKey(string key, string marcador)
        {
            var valor = ObtenerValorKey(key, marcador);
            return long.TryParse(valor, out var numero) ? numero : 0;
        }

        private static long ObtenerIdContactoProfesional(string key)
        {
            var id = ObtenerLongKey(key, "(Ic)");
            if (id > 0)
                return id;

            return ObtenerLongKey(key, "(ic)");
        }

        private static string ObtenerValorKey(string key, string marcador)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(marcador))
                return string.Empty;

            var inicio = key.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
            if (inicio < 0)
                return string.Empty;

            var finValorAnterior = inicio;
            var inicioValorAnterior = key.LastIndexOf(')', Math.Max(0, inicio - 1));
            inicioValorAnterior = inicioValorAnterior < 0 ? 0 : inicioValorAnterior + 1;

            var valorAnterior = key[inicioValorAnterior..finValorAnterior].Trim().Trim('|');
            if (!string.IsNullOrWhiteSpace(valorAnterior))
                return valorAnterior;

            inicio += marcador.Length;
            var fin = key.IndexOf('(', inicio);
            return (fin < 0 ? key[inicio..] : key[inicio..fin]).Trim().Trim('|');
        }

        private static string ObtenerTipoProfesional(string tag)
            => tag.Contains("Abog", StringComparison.OrdinalIgnoreCase) ? "A" : "I";

        private static string ObtenerOrigenOperacionesProfesional(string tag)
        {
            if (tag.Contains("Zona", StringComparison.OrdinalIgnoreCase))
                return "Zona";

            return tag.Contains("Empresa", StringComparison.OrdinalIgnoreCase) ? "Empresa" : "PF";
        }

        private static bool EsOperacionEjecutada(string tag)
            => tag.EndsWith("Eje", StringComparison.OrdinalIgnoreCase);
    }
}
