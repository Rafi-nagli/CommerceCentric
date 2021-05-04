$.ajaxSetup({
    converters: {
        'text json': jsonDateParser.parseJsonDate
    }
});
