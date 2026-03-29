(() => {
  const body = document.body;
  const successMessage = body?.dataset?.successMessage;
  const errorMessage = body?.dataset?.errorMessage;

  const showAlert = (icon, title, text) => {
    if (window.Swal) {
      Swal.fire({
        icon,
        title,
        text,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#2563eb',
        background: '#0f172a',
        color: '#f8fafc'
      });
      return;
    }

    window.alert(text);
  };

  if (successMessage) {
    showAlert('success', 'Operación completada', successMessage);
  }

  if (errorMessage) {
    showAlert('error', 'No fue posible completar la operación', errorMessage);
  }

  document.querySelectorAll('[data-confirm-message]').forEach(button => {
    button.addEventListener('click', event => {
      event.preventDefault();
      const form = button.closest('form');
      const message = button.dataset.confirmMessage;
      const title = button.dataset.confirmTitle || '¿Deseas continuar?';

      if (!window.Swal || !form) {
        form?.submit();
        return;
      }

      Swal.fire({
        title,
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, continuar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        cancelButtonColor: '#475569',
        background: '#0f172a',
        color: '#f8fafc'
      }).then(result => {
        if (result.isConfirmed) {
          form.submit();
        }
      });
    });
  });
})();
