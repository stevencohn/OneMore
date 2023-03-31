function HighlightActive(id) {
  if (typeof (id) != 'undefined') {
    if (id != null) {
      $(`#${id}`).addClass('active');
    }
  }
}
