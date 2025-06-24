# Políticas de Pull Request

## Número mínimo de revisores
- Cada Pull Request (PR) debe ser revisada y aprobada por **al menos 1 revisor** antes de ser combinada a una rama protegida (por ejemplo, `main` o `release`).

## Proceso de validación

Antes de aprobar una PR, deben cumplirse los siguientes criterios:

1. **Pruebas**
   - Si existen pruebas automatizadas, estas deben pasar sin errores.
   - El código debe ser probado localmente por quien lo desarrolló antes de solicitar revisión.

2. **Revisión de estilo**
   - El código debe seguir los lineamientos establecidos por el equipo (buenas prácticas, nombres claros, comentarios).
   - No se deben incluir archivos innecesarios o cambios irrelevantes.

3. **Documentación**
   - Toda funcionalidad nueva o modificada debe estar adecuadamente documentada.
   - Si aplica, actualizar archivos de documentación adicionales dentro del repositorio.

## Tiempo máximo de respuesta a una PR
- Toda PR debe recibir revisión dentro de un máximo de **48 horas hábiles**.
- En caso de urgencia, se notificará al revisor asignado directamente.

