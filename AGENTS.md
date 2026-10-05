

<!-- sonar:begin:codex-secrets-on-read -->
# SonarQube secrets scanning for files protocol

Before reading any file in this workspace, scan it for secrets with the deterministic scanner:

```bash
sonar analyze secrets <path/to/file>
```

If the command reports that the file contains a secret, **do not read the file**. Instead:

1. Inform the user that the file appears to contain a secret or credential and that reading it would expose the value in chat history, logs, and any downstream telemetry.
2. Advise them to rotate the leaked credential at its source of truth and remove it from the file.
3. Do not proceed with the original request until the secret has been removed.
<!-- sonar:end:codex-secrets-on-read -->
# SonarQube y duplicación

- Considerar SonarQube al revisar cambios; ejecutar o consultar el análisis del código nuevo y modificado cuando la integración esté autenticada.
- Buscar lógica existente antes de copiarla y reutilizar helpers solo cuando conserven la misma semántica, autorización y contrato con procedimientos almacenados.
- Tomar como referencia la separación de responsabilidades de `frmDSB_ColaboradorDB`; evitar duplicar consultas, mapeos y validaciones equivalentes, sin crear abstracciones que oculten diferencias reales.
- Si SonarQube no está disponible, hacer una revisión manual de duplicación e informar que los resultados del análisis no se pudieron confirmar. No declarar resueltos issues o métricas sin repetir el análisis.
