class Resource {
    public int x;
    public int y;
    public int resourceLeft;
    

    public Resource(int x, int y) {
        this.x = x;
        this.y = y;
        this.resourceLeft = 10;
    }

    public Resource(int x, int y, int ra) {
        this.x = x;
        this.y = y;
        this.resourceLeft = ra;
    }
}