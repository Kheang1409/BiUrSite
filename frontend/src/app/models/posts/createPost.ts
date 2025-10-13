export class CreatePost {
  #text!: string;
  #data!: Uint8Array;

  get text(): string {
    return this.#text;
  }
  set text(value: string) {
    this.#text = value;
  }

  get data(): Uint8Array {
    return this.#data;
  }

  set data(value: Uint8Array) {
    this.#data = value;
  }

  toJSON() {
    return {
      text: this.#text,
      data: this.#data,
    };
  }
}
