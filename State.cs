namespace Chimera {

    enum State {
        NORMAL,
        READING_COMMENT_LINE,
        READING_COMMENT_BLOCK,
        READING_STRING_FIRST_QUOTE,
        READING_STRING_SECOND_QUOTE
    }
}