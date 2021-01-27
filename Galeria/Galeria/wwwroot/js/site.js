// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

<script>
    var elem = document.querySelector('.grid');
    var msnry = new Masonry(elem, {
    itemSelector: '.grid-item',
        columnWidth: 200
    });
 </script>
